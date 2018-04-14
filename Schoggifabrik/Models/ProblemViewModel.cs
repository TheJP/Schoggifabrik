namespace Schoggifabrik.Models
{
    public class ProblemViewModel
    {
        public string Flavor { get; }
        public string Input { get; }
        public string Output { get; }
        public string StubCode { get; }

        public ProblemViewModel(string flavor, string input, string output, string stubCode)
        {
            Flavor = flavor;
            Input = input;
            Output = output;
            StubCode = stubCode;
        }
    }
}
