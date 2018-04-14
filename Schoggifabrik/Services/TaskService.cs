using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class TaskService
    {
        private const int MaxNumberOfTasks = 20;
        private const int MillisecondsCompileDuration = 30 * 1000;
        private const int MillisecondsTestCaseDuration = 30 * 1000;
        private const int MillisecondsVolumeDeleteDuration = 30 * 1000;

        private const string CompileCommand = "run --rm --network none -v {}:/hs/code.hs:ro -v volume-{}:/hs/output -w /hs haskell:8 bash -c \"ghc code.hs && mv code output/runnable\" > {} 2>&1";
        private const string RunCommand = "run -i --rm --network none -v volume-{}:/hs:ro -w /hs haskell:8 ./runnable < {} | head --bytes=3M > {} 2> {}";
        private const string DeleteVolumeCommand = "volume rm volume-{}";

        private const string CodeFileName = "code.hs";
        private const string CompileLog = "compile.log";
        private const string OutputFileName = "output.log";
        private const string RunErrorLog = "error.log";

        private readonly ILogger<TaskService> logger;
        private readonly ConcurrentDictionary<string, TaskData> tasks = new ConcurrentDictionary<string, TaskData>();
        private readonly DirectoryInfo storageRoot;

        public TaskService(ILogger<TaskService> logger, IConfiguration configuration)
        {
            this.logger = logger;

            var storageRoot = configuration.GetValue("Docker:StorageRoot", "Data");
            this.storageRoot = Directory.CreateDirectory(storageRoot);
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
                var compileLog = Path.Combine(taskStorage.FullName, CompileLog);
                RunDockerCommand(string.Format(CompileCommand, codeFileName, task.TaskId, compileLog), MillisecondsCompileDuration);

                logger.LogInformation("Start to run test cases for task {}", task.TaskId);
                var outputLog = Path.Combine(taskStorage.FullName, OutputFileName);
                var errorLog = Path.Combine(taskStorage.FullName, RunErrorLog);
                //RunDockerCommand(string.Format(RunCommand, task.TaskId, inputFileName, outputLog, errorLog));
            }
            finally
            {
                logger.LogInformation("Start cleanup for task {}", task.TaskId);
                RunDockerCommand(string.Format(DeleteVolumeCommand, task.TaskId), MillisecondsVolumeDeleteDuration);
                // Keeping code files on purpose
            }
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
        /// <param name="command">Docker command without the "docker" prefix.</param>
        /// <param name="milliseconds">Maximal time that this command is allowed to run.</param>
        private void RunDockerCommand(string command, int milliseconds)
        {
            var process = new Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = true;

            process.Start();
            if (!process.WaitForExit(milliseconds) || process.ExitCode != 0)
            {
                // Log failed task
                if (!process.HasExited) { logger.LogWarning("Task exeeded timeout"); }
                else { logger.LogWarning("Task had non zero exit code {}", process.ExitCode); }

                process.Kill();
                throw new TaskTimeOutException();
            }
        }
    }

    [Serializable]
    internal class TooManyTasksException : Exception
    {
        public TooManyTasksException() { }
        public TooManyTasksException(string message) : base(message) { }
        public TooManyTasksException(string message, Exception innerException) : base(message, innerException) { }
        protected TooManyTasksException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    internal class TaskTimeOutException : Exception
    {
        public TaskTimeOutException() { }
        public TaskTimeOutException(string message) : base(message) { }
        public TaskTimeOutException(string message, Exception innerException) : base(message, innerException) { }
        protected TaskTimeOutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
