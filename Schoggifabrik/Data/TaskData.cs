using System;

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

        /// <summary>
        /// Current state of this task.
        /// </summary>
        public TaskState State { get; }

        /// <summary>
        /// Result of the task. Only available if <see cref="State"/> is <see cref="TaskState.Done"/>.
        /// </summary>
        public TaskResult Result { get; }

        public TaskData(Problem problem, string code)
        {
            TaskId = Guid.NewGuid().ToString();
            Problem = problem;
            Code = code;
            State = TaskState.Preparing;
            Result = null;
        }

        private TaskData(string taskId, Problem problem, string code, TaskState state, TaskResult result)
        {
            TaskId = taskId;
            Problem = problem;
            Code = code;
            State = state;
            Result = result;
        }

        /// <summary>Create new instance of immutable <see cref="TaskData"/> with changed state.</summary>
        /// <param name="state">New state to set.</param>
        private TaskData SetState(TaskState state)
        {
            if (state <= State) { throw new ArgumentException("The given state is invalid: State can only advance forward"); }
            return new TaskData(TaskId, Problem, Code, state, Result);
        }

        public TaskData SetStateCompiling() => SetState(TaskState.Compiling);
        public TaskData SetStateRunning() => SetState(TaskState.Running);
        public TaskData SetStateDone(TaskResult result) => new TaskData(TaskId, Problem, Code, TaskState.Done, result);

        public enum TaskState { Preparing, Compiling, Running, Done }

        public interface TaskResult { }
        public interface FailedTaskResult : TaskResult { }
        public interface TimeoutResult : FailedTaskResult { }
        public class CompilationTimeout : TimeoutResult { }
        public class RunTimeout : TimeoutResult { }
        public abstract class ErrorResult : FailedTaskResult
        {
            public string Error { get; }
            public ErrorResult(string error) => Error = error;
        }
        public class CompilationError : ErrorResult
        {
            public CompilationError(string error) : base(error) { }
        }
        public class RunError : ErrorResult
        {
            public RunError(string error) : base(error) { }
        }
        public class WrongResult : FailedTaskResult { }
        public class Success : TaskResult { }
    }
}
