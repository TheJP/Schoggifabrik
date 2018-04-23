using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schoggifabrik.Data
{
    public class TemperatureTestCase : Problem.TestCase
    {
        public TemperatureTestCase(IEnumerable<(int, int)> input) : base(ConvertInput(input), CreateOutputTest(input)) { }

        public TemperatureTestCase(params (int, int)[] input) : this((IEnumerable<(int, int)>)input) { }

        private static string ConvertInput(IEnumerable<(int, int)> input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(input.Count());
            stringBuilder.AppendLine();
            foreach (var (a, b) in input)
            {
                stringBuilder.AppendFormat("{0} {1}", a, b);
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }

        private static Func<string, bool> CreateOutputTest(IEnumerable<(int, int)> input)
        {
            var expected = CalculateResult(input).ToArray();
            return actual =>
            {
                var actualOptions = actual
                    .Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(number => int.TryParse(number, out var parsed) ? Option.Some(parsed) : Option.None<int>());
                if (actualOptions.Any(option => !option.HasValue)) { return false; }
                if (actualOptions.Count() % 2 != 0) { return false; }

                var actualPairs = Pairs(actualOptions.Select(option => option.ValueOr(0))).ToArray();
                if (actualPairs.Length != expected.Length) { return false; }

                for (int i = 0; i < expected.Length; ++i)
                {
                    if (!expected[i].Equals(actualPairs[i])) { return false; }
                }

                return true;
            };
        }

        private static IEnumerable<(int, int)> CalculateResult(IEnumerable<(int, int)> input)
        {
            var ordered = input.OrderBy(x => x.Item1).ToArray();
            var current = ordered[0];

            for (int i = 1; i < ordered.Length; ++i)
            {
                var (new1, new2) = ordered[i];
                if (current.Item2 >= new1) { current = (current.Item1, Math.Max(current.Item2, new2)); }
                else
                {
                    yield return current;
                    current = ordered[i];
                }
            }
            yield return current;
        }

        private static IEnumerable<(int, int)> Pairs(IEnumerable<int> input)
        {
            var enumerator = input.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var a = enumerator.Current;
                enumerator.MoveNext();
                yield return (a, enumerator.Current);
            }
        }
    }
}
