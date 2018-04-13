using Microsoft.Extensions.Logging;
using Schoggifabrik.Data;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Schoggifabrik.Services
{
    public class TaskService
    {
        private const int MaxNumberOfTasks = 20;
        private const int MillisecondsCompileDuration = 30 * 1000;
        private const int MillisecondsTestCaseDuration = 30 * 1000;

        private const string CompileCommand = "run --rm --network none -v {}:/hs/code.hs:ro -v volume-{}:/hs/output -w /hs haskell:8 bash -c \"ghc code.hs && mv code output/runnable\"";
        private const string RunCommand = "run -i --rm --network none -v volume-{}:/hs:ro -w /hs haskell:8 ./runnable < {} | head --bytes=3M > {}";

        private readonly ILogger<TaskService> logger;
        private readonly ConcurrentDictionary<string, TaskData> tasks = new ConcurrentDictionary<string, TaskData>();

        public TaskService(ILogger<TaskService> logger) => this.logger = logger;

        /// <summary>
        /// Creates and runs a task for the given problem number using the given code.
        /// </summary>
        /// <param name="problemNumber">Number of the problem that this code tries to solve.</param>
        /// <param name="clientCode">Code that tries to slve the provlem.</param>
        /// <returns>Id of the created task.</returns>
        public string CreateTask(int problemNumber, string clientCode)
        {
            var task = new TaskData(problemNumber, clientCode);
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
                // TODO: Save code as file
                logger.LogInformation("Start to compile task {}", task.TaskId);
                // TODO: Compile
                //RunDockerCommand(string.Format(CompileCommand, ))
                logger.LogInformation("Start to run test cases for task {}", task.TaskId);
                // TODO: Run all test cases
            }
            finally
            {
                logger.LogInformation("Start cleanup for task {}", task.TaskId);
                // TODO: Cleanup
            }
        }

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
