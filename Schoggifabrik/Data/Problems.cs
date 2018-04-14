using System.Collections.Generic;

namespace Schoggifabrik.Data
{
    public static class Problems
    {
        public static IList<Problem> AsList { get; } = new Problem[]
        {
            new Problem(
                flavor: "<p>Wir wissen, dass du noch keine Anstellung an deinem neuen Wohnort gefunden hast. " +
                    "Aber keine Angst: Die <span class=\"highlight\">Schoggifabrik</span> vorort stellt dich, den beinahe schon Schweizer, sehr gerne ein. "+
                    "Da du in deinem Portfolio schreibst es falle dir leicht neue Programmiersprachen zu lernen, " +
                    "sind sie sich auch sicher, dass du dich schnell in ihrem <span class=\"highlight\">Haskell</span> Entwicklungsstack zurechtfindest.</p>" +
                    "<p>Deine erste Aufgabe ist eine freundliche Willkommensnachricht auf dem Bildschirm im Visitor Centre darzustellen. " +
                    "Schreibe dazu Haskell Programm das <span class=\"highlight\">\"Herzlich Willkommen!\"</span> ausgibt.</p>",
                input: "Keinen Input",
                output: "Den exakten String <span class=\"highlight\">\"Herzlich Willkommen!\"</span> (ohne Anführungszeichen). " +
                    "Dieser muss auf dem Standard-Output ausgegeben werden.",
                stubCode: "-- Hier den Code einfügen der die Begrüssungsnachricht ausgibt\n"+
                    "-- Der Code muss in Haskell geschrieben werden",
                testCases: new Problem.TestCase[]{ })
        };
    }
}
