﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Optional;
using Schoggifabrik.Data;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Schoggifabrik.Services
{
    /// <summary>
    /// Service that handles the execution of the given tasks.
    /// </summary>
    public class TaskService
    {
        private const int MaxNumberOfTasks = 20;
        private const int MillisecondsCompileDuration = 30 * 1000;
        private const int MillisecondsTestCaseDuration = 30 * 1000;
        private const int MillisecondsVolumeDeleteDuration = 5 * 1000;

        private const string CodeFileName = "code.hs";
        private const string CompileLog = "compile.log";
        private const string RunOutputLog = "output.log";
        private const string RunErrorLog = "error.log";

        private readonly ILogger<TaskService> logger;
        private readonly ConcurrentDictionary<string, TaskData> tasks = new ConcurrentDictionary<string, TaskData>();
        private readonly ConcurrentDictionary<string, TaskData> completedTasks = new ConcurrentDictionary<string, TaskData>();

        private readonly DirectoryInfo storageRoot;
        private readonly string shell;

        public TaskService(ILogger<TaskService> logger, IConfiguration configuration)
        {
            this.logger = logger;

            var storageRoot = configuration.GetValue("Docker:StorageRoot", "TaskData");
            this.storageRoot = Directory.CreateDirectory(storageRoot);

            shell = configuration.GetValue("SCHOGGIFABRIK:SHELL", "bash");
        }

        private string CompileCommand(string codeFileName, string taskId, string compileLog) =>
            $"docker run --rm --network none -v \"{codeFileName}\":/hs/code.hs:ro -v volume-{taskId}:/hs/output -w //hs haskell:8 bash -c \"ghc code.hs && mv code output/runnable\" > \"{compileLog}\" 2>&1";

        private string RunCommand(string taskId, string outputLog, string errorLog) =>
            $"docker run -i --rm --network none -v volume-{taskId}:/hs:ro -w //hs haskell:8 ./runnable > >(head --bytes=3M | tee \"{outputLog}\") 2> >(head --bytes=3M | tee \"{errorLog}\" >&2)";

        private string DeleteVolumeCommand(string taskId) =>
            $"docker volume rm volume-{taskId}";

        /// <summary>Updates a task in <see cref="tasks"/>. Assumes the task exists in the dictionary!</summary>
        /// <param name="taskId">Id of the task which should be updated.</param>
        /// <param name="update">Update function. (Might get called more than once!)</param>
        /// <returns>New task instance after update.</returns>
        private TaskData UpdateTask(string taskId, Func<TaskData, TaskData> update)
        {
            var task = update(tasks[taskId]);
            return tasks.AddOrUpdate(taskId, task, (_, oldTask) => update(oldTask));
        }

        /// <summary>
        /// Creates and runs a task for the given problem number using the given code.
        /// </summary>
        /// <param name="problem">Problem that this code tries to solve.</param>
        /// <param name="clientCode">Code that tries to slve the provlem.</param>
        /// <returns>Id of the created task.</returns>
        public string CreateTask(Problem problem, string clientCode)
        {
            var task = new TaskData(problem, clientCode);
            if (tasks.Count >= MaxNumberOfTasks)
            {
                throw new TooManyTasksException();
            }

            Task.Run(() => RunTask(task));

            return task.TaskId;
        }

        private void RunTask(TaskData task)
        {
            if (!tasks.TryAdd(task.TaskId, task))
            {
                throw new InvalidOperationException();
            }

            try
            {
                logger.LogInformation("Storing code as file for task {}", task.TaskId);
                var (taskStorage, codeFileName) = CreateFiles(task);

                logger.LogInformation("Start to compile task {}", task.TaskId);
                task = UpdateTask(task.TaskId, _ => _.SetStateCompiling());
                var compileSuccess = Compile(task, taskStorage, codeFileName);
                if (!compileSuccess) { return; }

                logger.LogInformation("Start to run test cases for task {}", task.TaskId);
                task = UpdateTask(task.TaskId, _ => _.SetStateRunning());
                for (int i = 0; i < task.Problem.TestCases.Count; ++i)
                {
                    bool runSuccess = RunTestCase(task, taskStorage, i);
                    if (!runSuccess) { return; }
                }

                task = UpdateTask(task.TaskId, _ => _.SetStateDone(new TaskData.Success()));
            }
            finally
            {
                // Keeping code files on purpose
                logger.LogInformation("Start cleanup for task {}", task.TaskId);
                RunDockerCommand(DeleteVolumeCommand(task.TaskId), MillisecondsVolumeDeleteDuration);

                if (tasks.TryRemove(task.TaskId, out task)) { completedTasks.TryAdd(task.TaskId, task); }
                logger.LogInformation("Task {} ended. Result: {}", task.TaskId, task.Result.ToString());
            }
        }

        /// <summary>
        /// Compile the client code in a docker container.
        /// Sets the task to Done if an error occurs.
        /// </summary>
        /// <param name="task">Task to compile.</param>
        /// <param name="taskStorage">Folder in which logs and input code are stored for this task.</param>
        /// <param name="codeFileName">Name of the source code file.</param>
        /// <returns>true=success, false=failed.</returns>
        private bool Compile(TaskData task, DirectoryInfo taskStorage, string codeFileName)
        {
            var compileLog = Path.Combine(taskStorage.FullName, CompileLog);
            var compileResult = RunDockerCommand(CompileCommand(codeFileName, task.TaskId, compileLog), MillisecondsCompileDuration);
            compileResult.MatchNone(error =>
            {
                var result = error is FailedCommandError commandError ?
                    new TaskData.CompilationError(commandError.ErrorLog) :
                    new TaskData.CompilationTimeout() as TaskData.TaskResult;
                UpdateTask(task.TaskId, _ => _.SetStateDone(result));
            });
            return compileResult.HasValue;
        }

        /// <summary>
        /// Run the given test case in a docker container.
        /// Sets the task to Done if an error occurs.
        /// </summary>
        /// <param name="task">Task for which a test case should be executed.</param>
        /// <param name="taskStorage">Folder in which logs and input code are stored for this task.</param>
        /// <param name="testCaseIndex">Index of the test case that should be executed.</param>
        /// <returns>true=success, false=failed.</returns>
        private bool RunTestCase(TaskData task, DirectoryInfo taskStorage, int testCaseIndex)
        {
            // Run test case in docker container
            var test = task.Problem.TestCases[testCaseIndex];
            var outputLog = Path.Combine(taskStorage.FullName, $"{testCaseIndex}.{RunOutputLog}");
            var errorLog = Path.Combine(taskStorage.FullName, $"{testCaseIndex}.{RunErrorLog}");
            var runResult = RunDockerCommand(RunCommand(task.TaskId, outputLog, errorLog), MillisecondsTestCaseDuration, test.Input);

            // Handle result of test run
            var runSuccess = runResult.Match(
                some =>
                {
                    if (!test.TestOutput(some))
                    {
                        UpdateTask(task.TaskId, _ => _.SetStateDone(new TaskData.WrongResult()));
                        return false;
                    }
                    else { return true; }
                },
                error =>
                {
                    var result = error is FailedCommandError commandError ?
                        new TaskData.RunError(commandError.ErrorLog) :
                        new TaskData.RunTimeout() as TaskData.TaskResult;
                    UpdateTask(task.TaskId, _ => _.SetStateDone(result));
                    return false;
                });
            return runSuccess;
        }

        /// <summary>
        /// Creates the task folder and task code file.
        /// </summary>
        /// <param name="task">Task information.</param>
        /// <returns>Task folder and code file name information.</returns>
        private (DirectoryInfo taskStorage, string codeFileName) CreateFiles(TaskData task)
        {
            var taskStorage = storageRoot.CreateSubdirectory(task.TaskId);
            var codeFileName = Path.Combine(taskStorage.FullName, CodeFileName);
            using (var writer = new StreamWriter(codeFileName, false, Encoding.UTF8))
            {
                writer.Write(task.Code);
            }
            return (taskStorage, codeFileName);
        }

        /// <summary>
        /// Run the given command for docker.
        /// </summary>
        /// <param name="command">Docker command.</param>
        /// <param name="milliseconds">Maximal time that this command is allowed to run.</param>
        /// <returns>Some(output) if the run was successful, where output is all that was written to stdout. None(<see cref="CommandError"/>) otherwise.</returns>
        private Option<string, CommandError> RunDockerCommand(string command, int milliseconds, string input = null)
        {
            // Setup process
            logger.LogDebug("{} -c '{}'", shell, command);
            var process = new Process();
            process.StartInfo.FileName = shell;
            process.StartInfo.Arguments = $"-c '{command}'";
            process.StartInfo.UseShellExecute = false;

            // Redirect all input and output
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.Start();

            // Send given input to created process
            if (input != null)
            {
                process.StandardInput.Write(input);
                process.StandardInput.Flush();
            }

            // Handle error cases
            if (!process.WaitForExit(milliseconds) || process.ExitCode != 0)
            {
                CommandError error;
                if (process.HasExited)
                {
                    logger.LogWarning("Task had non zero exit code {}", process.ExitCode);
                    error = new FailedCommandError(process.StandardError.ReadToEnd());
                }
                else
                {
                    logger.LogWarning("Task exeeded timeout");
                    process.Kill();
                    error = new TimeoutCommandError();
                }
                return Option.None<string, CommandError>(error);
            }
            // Handle successful runs
            else
            {
                return process.StandardOutput.ReadToEnd()
                    .Some<string, CommandError>();
            }
        }

        public interface CommandError { }
        public class FailedCommandError : CommandError
        {
            public string ErrorLog { get; }
            public FailedCommandError(string errorLog) => ErrorLog = errorLog;
        }
        public class TimeoutCommandError : CommandError { }
    }

    [Serializable]
    internal class TooManyTasksException : Exception
    {
        public TooManyTasksException() { }
        public TooManyTasksException(string message) : base(message) { }
        public TooManyTasksException(string message, Exception innerException) : base(message, innerException) { }
        protected TooManyTasksException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
