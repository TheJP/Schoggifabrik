using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schoggifabrik.Data
{
    /// <summary>
    /// Class that holds all the data for a grading task.
    /// </summary>
    public class TaskData
    {
        /// <summary>
        /// Unique id of the task.
        /// </summary>
        public string TaskId { get; }

        /// <summary>
        /// Problem that this task tries to solve.
        /// </summary>
        public Problem Problem { get; }

        /// <summary>
        /// Source code that this task runs on.
        /// </summary>
        public string Code { get; }

        public TaskData(Problem problem, string code)
        {
            TaskId = Guid.NewGuid().ToString();
            Problem = problem;
            Code = code;
        }
    }
}
