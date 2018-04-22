using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Schoggifabrik.Data.Problem;

namespace Schoggifabrik.Data
{
    public class StatisticsTestCase : TestCase
    {
        public StatisticsTestCase(IEnumerable<int> input) : base(ConvertInput(input), CreateOutputTest(input)) { }

        private static string ConvertInput(IList<int> input) => String.Join(' ', input);

        private static Func<string, bool> CreateOutputTest(IList<int> input)
        {
            var expected = new double[] {
                input.Min(),
                input.Max(),
                input.Average(),
                //TODO
                input.Count,
                input.Sum(),
            };

            return actual =>
            {
                //TODO: actual.Split();
                return true;
            };
        }

        private static double Median(IList<int> input) =>
            input.Count % 2 == 1 ?
                input[input.Count / 2] :
                (input[input.Count / 2] + input[input.Count / 2 - 1]) / 2.0;
    }
}
