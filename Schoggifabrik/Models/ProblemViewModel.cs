namespace Schoggifabrik.Models
{
    public class ProblemViewModel
    {
        public string Name { get; }
        public string Flavor { get; }
        public string Input { get; }
        public string Output { get; }
        public string StubCode { get; }

        public ProblemViewModel(string name, string flavor, string input, string output, string stubCode)
        {
            Name = name;
            Flavor = flavor;
            Input = input;
            Output = output;
            StubCode = stubCode;
        }
    }
}
