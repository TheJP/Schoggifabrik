using System;
using System.Collections.Generic;

namespace Schoggifabrik.Data
{
    public class Problem
    {
        public string Flavor { get; }
        public string Input { get; }
        public string Output { get; }
        public string StubCode { get; }
        public IList<TestCase> TestCases { get; }

        public Problem(string flavor, string input, string output, string stubCode, IList<TestCase> testCases)
        {
            Flavor = flavor;
            Input = input;
            Output = output;
            StubCode = stubCode;
            TestCases = testCases;
        }

        public class TestCase
        {
            public string Input { get; }
            public Func<string, bool> TestOutput { get; }

            public TestCase(string input, Func<string, bool> testOutput)
            {
                Input = input;
                TestOutput = testOutput;
            }
        }
    }
}
