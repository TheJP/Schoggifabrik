using System;
using System.Collections.Generic;
using System.Linq;

namespace Schoggifabrik.Data
{
    public static class Problems
    {
        private static bool CompareLax(string expected, string actual) => expected.Trim().Normalize().Equals(
            actual?.Trim()?.Normalize(), StringComparison.OrdinalIgnoreCase);

        private static Problem.TestCase Match(string input, string expected) =>
            new Problem.TestCase(input, actual => CompareLax(expected, actual));

        public static int Count => AsList.Count;

        public static IList<Problem> AsList { get; } = new Problem[]
        {
            new Problem(
                name: "Willkommen",
                flavor: "Wir wissen, dass du noch keine Anstellung an deinem neuen Wohnort gefunden hast. " +
                    "Aber keine Angst: Die <span class=\"highlight\">Schoggifabrik</span> vorort stellt dich, den beinahe schon Schweizer, sehr gerne ein. "+
                    "Da du in deinem Portfolio schreibst es falle dir leicht neue Programmiersprachen zu lernen, " +
                    "sind sie sich auch sicher, dass du dich schnell in ihrem <span class=\"highlight\">Haskell</span> Entwicklungsstack zurechtfindest.</p>" +
                    "<p>Deine erste Aufgabe ist eine freundliche Willkommensnachricht auf dem Bildschirm im Visitor Centre darzustellen. " +
                    "Schreibe dazu Haskell Programm das <span class=\"highlight\">\"Herzlich Willkommen!\"</span> ausgibt.",
                input: "Keinen Input",
                output: "Den exakten String <span class=\"highlight\">\"Herzlich Willkommen!\"</span> (ohne Anführungszeichen). " +
                    "Dieser muss auf dem Standard-Output ausgegeben werden.",
                stubCode: "-- Hier den Code einfügen der die Begrüssungsnachricht ausgibt\n"+
                    "-- Der Code muss in Haskell geschrieben werden",
                testCases: new[] {
                    Match(null, "Herzlich Willkommen!")
                }),

            new Problem(
                name: "Willkommen Persönlich",
                flavor: "Gut gemacht. Dein Vorgesetzter Gummi freut sich über die neue Willkommensnachricht. " +
                    "Er gibt allerdings zu, dass diese einfache Aufgabe nur ein kleiner Test war um zu prüfen, " +
                    "ob du gewappnet für das Berufsleben als Schoggifabriksoftwareentwickler bist.</p>" +
                    "<p>Die Begrüssung im Visitor Centre sieht zwar toll aus, ist aber unpersönlich und immer gleich. " +
                    "Erweitere dein Haskell Programm um <span class=\"highlight\">Gäste mit ihrem Namen anzusprechen</span>.",
                input: "Die 1. Inputzeile enthält jeweils den Namen mit welchem der Gast begrüsst werden soll.</p>" +
                    "<p>Beispiele: <code>Paul</code> oder <code>Herr Gummi</code>",
                output: "Den String <span class=\"highlight\">\"Willkommen NAME!\"</span>, wobei NAME durch den Namen zu ersetzen ist.</p>" +
                    "<p>Beispiel Output: <code>Willkommen Paul!</code> oder <code>Willkommen Herr Gummi!</code>",
                stubCode: "main = putStrLn \"Herzlich Willkommen!\"",
                testCases: new[] {
                    Match("Paul", "Willkommen Paul!"),
                    Match("Herr Gummi", "Willkommen Herr Gummi!"),
                    Match("Mr.  Sonderzeichen !&-*#@", "Willkommen Mr.  Sonderzeichen !&-*#@!"),
                }),

            new Problem(
                name: "Statistiken",
                flavor: "Die Willkommensnachricht zeigt jetzt allen Gästen persönliche Begrüssungen und deine Arbeit dafür ist getan. " +
                    "Als nächstes braucht die Schoggiverpackungs-Abteilung deine Hilfe. Dort wurde kürzlich eine neue Maschine angeschafft, " +
                    "welche für jede verpackte Schoggi die Verpackungsqualität misst und mit einem Wert von 0 - 100 angibt.</p>" +
                    "<p>Du sollst aus diesen Werten nun <span class=\"highlight\">Statistiken</span> berechnen. Konkret sollen " +
                    "<span class=\"highlight\">Minumum, Maximum, Durchschnitt, Median, Anzahl und Summe</span> " +
                    "berechnet und in dieser Reihenfolge ausgegeben werden. Dein Arbeitskollege hat bereits die Eingabe programmiert.",
                input: "Die 1. Inputzeile enthält eine Reihe von Zahlen q<sub>1</sub>, q<sub>2</sub>, ..., q<sub>n</sub> die jeweilige Verpackungsqualität. " +
                    "Wobei 0 &le; q<sub>i</sub> &le; 100.</p>" +
                    "<p>Beispiele: <code>55</code> oder <code>1 2 3 6</code>",
                output: "Eine Reihe von Zahlen getrennt durch Leerzeichen. Nämlich: <span class=\"highlight\">" +
                    "Minumum, Maximum, Durchschnitt, Median, Anzahl und Summe</span> der eingegebenen Qualitäten.</p>" +
                    "<p>Beispiel Output: <code>55 55 55 55 1 55</code> oder <code>1 6 3 2.5 4 12</code>",
                stubCode: "main = do\n"+
                    "  values <- getLine >>= (return.fmap readDouble.words)\n" +
                    "  mapM_ print values\n\n" +
                    "readDouble :: String -> Double\n" +
                    "readDouble = read",
                testCases: new[] {
                    new StatisticsTestCase(55),
                    new StatisticsTestCase(1, 2, 3, 6),
                    new StatisticsTestCase(Enumerable.Range(0, 100).ToArray()),
                    new StatisticsTestCase(12, 54, 23, 91, 49, 98, 85, 97),
                }),

            new Problem(
                name: "Temperaturen",
                flavor: "Durch das Auswerten deiner Statistiken sind die Verpackungstechnologen zum Schluss gekommen, dass die momentane Verpackungsqualität Mängel aufweist. " +
                    "Nach genaueren Untersuchungen ist das Problem aufgefallen: Die Schoggi wurde nicht bei optimalen Temperaturen verpackt.</p>" +
                    "<p>Hilf den Verpackungstechnologen gültige <span class=\"highlight\">Temperaturbereiche</span> zu berechnen. " +
                    "Die Temperaturbereiche konkreter Maschinen und Schoggi sind bekannt, aber viel zu unübersichtlich durch viele Redundanzen und Überlappungen. " +
                    "Deine Aufgabe ist es, die erhaltenen Werte zusammenzufassen, bis keine Redundanzen oder Überlappungen mehr vorkommen.",
                input: "Die 1. Zeile enthält die Anzahl n der Tempraturbereiche. Die Zeilen 2 bis 2+n enthalten jeweils einen Temperaturbereich als Zahlenpaar min<sub>i</sub> max<sub>i</sub>. " +
                    "Wobei -100'000'000 &le; min<sub>i</sub> &lt; max<sub>i</sub> &le; 100'000'000 die Temperaturen in µ°C</p>" +
                    "<p><code class=\"code-block\">3\n3 5\n7 9\n1 3</code> oder <code class=\"code-block\">3\n0 100\n101 555\n-1 0</code>",
                output: "Die zusammengefassten Temperaturbereiche jeweils als 1 Zahlenpaar min<sub>j</sub> max<sub>j</sub> pro Zeile. Zum Beispiel:</p>" +
                    "<p><code class=\"code-block\">1 5\n7 9</code> oder <code class=\"code-block\">-1 100\n101 555</code>",
                stubCode: String.Join('\n', new []{
                    "main = do",
                    "  n <- fmap readInt getLine",
                    "  ranges <- getContents >>= (return.take n.clump.map readInt.words)",
                    "  let solution = ranges -- TODO: Implement",
                    "  mapM_ print solution",
                    "",
                    "readInt :: String -> Int",
                    "readInt = read",
                    "",
                    "clump :: [Int] -> [(Int, Int)]",
                    "clump (a:b:xs) = (a, b) : clump xs",
                }),
                testCases: new[] {
                    new TemperatureTestCase((3, 5), (7, 9), (1, 3)),
                    new TemperatureTestCase((0, 100), (101, 555), (-1, 0)),
                    new TemperatureTestCase((1, 100), (10, 15), (-1, 0)),
                    new TemperatureTestCase(Enumerable.Range(0, 1_000_000).Select(i => (i, i + 1))), // Merge everything
                    new TemperatureTestCase(Enumerable.Range(0, 10_000).Select(i => i * 2).Select(i => (i, i + 1))), // Merge nothing
                })
        };
    }
}
