using Schoggifabrik.Data;
using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Schoggifabrik.Services
{
    public class TaskService
    {
        private const int MaxNumberOfTasks = 20;

        private readonly ConcurrentDictionary<string, TaskData> tasks = new ConcurrentDictionary<string, TaskData>();

        public string CreateTask(int taskNumber, string clientCode)
        {
            if(tasks.Count > MaxNumberOfTasks)
            {
                throw new TooManyTasksException();
            }

            var taskId = Guid.NewGuid().ToString();
            var task = new TaskData();

            if(!tasks.TryAdd(taskId, task))
            {
                throw new InvalidOperationException(); // very low probability to happen
            }

            return taskId;
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
}
