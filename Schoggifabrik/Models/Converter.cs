using Schoggifabrik.Data;

namespace Schoggifabrik.Models
{
    public static class Converter
    {
        public static ProblemViewModel ToViewModel(this Problem problem) => new ProblemViewModel(problem.Name, problem.Flavor, problem.Input, problem.Output, problem.StubCode);
    }
}
