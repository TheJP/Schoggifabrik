using System;
using Schoggifabrik.Data;
using static Schoggifabrik.Data.TaskData;

namespace Schoggifabrik.Models
{
    public static class Converter
    {
        public static ProblemViewModel ToViewModel(this Problem problem) => new ProblemViewModel(problem.Name, problem.Flavor, problem.Input, problem.Output, problem.StubCode);

        public static TaskViewModel ToViewModel(this TaskData task) => new TaskViewModel(task.Problem.Name, TaskStatusText(task), TaskStatus(task));

        private static string TaskStatusText(TaskData task)
        {
            if (task.State != TaskState.Done)
            {
                return task.State.ToString();
            }

            switch (task.Result)
            {
                case CompilationTimeout _: return "Compilation Timeout";
                case CompilationError _: return "Compilation Error";
                case RunTimeout _: return "Timeout";
                case RunError _: return "Runtime Error";
                case WrongResult _: return "Wrong Result";
                case Success _: return "Success";
                default: return "Unkown State?";
            }
        }

        private static TaskViewModel.TaskStatus TaskStatus(TaskData task) =>
            task.State != TaskState.Done ? TaskViewModel.TaskStatus.Pending :
                (task.Result is Success ? TaskViewModel.TaskStatus.Success : TaskViewModel.TaskStatus.Error);
    }
}
