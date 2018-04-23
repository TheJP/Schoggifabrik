using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Schoggifabrik.Data.Problem;

namespace Schoggifabrik.Data
{
    public class StatisticsTestCase : TestCase
    {
        private const double Epsilon = 1e-3;

        public StatisticsTestCase(IEnumerable<int> input) : base(ConvertInput(input), CreateOutputTest(input)) { }

        public StatisticsTestCase(params int[] input) : this((IEnumerable<int>)input) { }

        private static string ConvertInput(IEnumerable<int> input) => String.Join(' ', input);

        private static Func<string, bool> CreateOutputTest(IEnumerable<int> input)
        {
            var expected = new double[] {
                input.Min(),
                input.Max(),
                input.Average(),
                Median(input.OrderBy(x => x).ToArray()),
                input.Count(),
                input.Sum(),
            };

            return actual =>
            {
                var actualOptions = actual
                    .Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(number => double.TryParse(number, out var parsed) ? Option.Some(parsed) : Option.None<double>());
                if (actualOptions.Any(option => !option.HasValue)) { return false; }

                var actualNumbers = actualOptions.Select(option => option.ValueOr(0.0)).ToArray();
                if (actualNumbers.Length != expected.Length) { return false; }

                for (int i = 0; i < expected.Length; ++i)
                {
                    if (Math.Abs(expected[i] - actualNumbers[i]) > Epsilon) { return false; }
                }

                return true;
            };
        }

        private static double Median(IList<int> input) =>
            input.Count % 2 == 1 ?
                input[input.Count / 2] :
                (input[input.Count / 2] + input[input.Count / 2 - 1]) / 2.0;
    }
}
