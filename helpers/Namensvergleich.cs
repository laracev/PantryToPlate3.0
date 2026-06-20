using System;
using System.Globalization;

namespace PantryToPlate.helpers
{
    public static class Namensvergleich
    {
        public static double ZahlLesen(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            text = text.Trim().Replace(" kcal", "").Replace("g", "").Replace(" ", "");

            if (text.Contains(",") && !text.Contains("."))
            {
                text = text.Replace(",", ".");
            }
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return 0;
        }


        //chatgpt, promt: Schreibe eine Funktion, die aus einem Text die Menge in Gramm oder Milliliter extrahiert
        public static double MengeAusText(string text)
        {
            int start = text.LastIndexOf('(');
            int ende = text.IndexOf(')', start + 1);
            if (start >= 0 && ende > start)
            {
                string mengenText = text.Substring(start + 1, ende - start - 1).ToLower().Trim();
                double zahl = ZahlLesen(mengenText);
                if (mengenText.Contains("kg"))
                {
                    zahl *= 1000;
                }
                else if (mengenText.Contains("l") && !mengenText.Contains("ml"))
                {
                    zahl *= 1000;
                }
                if (zahl > 0)
                {
                    return zahl;
                }
            }
            return 100;
        }
        //chatgpt ende


        //chatgpt, promt: Schreibe eine Funktion, die einen Namen normalisiert, indem sie Umlaute ersetzt, bestimmte Wörter entfernt
        public static string NormalisiereName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }
        
            name = name.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");
            name = name.Replace(",", " ").Replace("/", " ").Replace("-", " ");

            string[] woerter = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string ergebnis = "";
            foreach (string w in woerter)
            {
                string wort = w.Trim();
                if (wort == "roh" || wort == "gekocht" || wort == "geschält" || wort == "geschaelt" ||
                    wort == "frisch" || wort == "getrocknet" || wort == "tiefgekuehlt" || wort == "tk" ||
                    wort == "klein" || wort == "gross" || wort == "groß" || wort == "gehackt" ||
                    wort == "gewuerfelt" || wort == "gewürfelt" || wort == "gerieben" ||
                    wort == "bio" || wort == "dose" || wort == "konserve")
                {
                    continue;
                }

                if (wort.Length > 5 && wort.EndsWith("n"))
                {
                    wort = wort.Substring(0, wort.Length - 1);
                }
                if (wort.Length > 5 && wort.EndsWith("en"))
                {
                    wort = wort.Substring(0, wort.Length - 2);
                }
                if (wort.Length > 5 && wort.EndsWith("e"))
                {
                    wort = wort.Substring(0, wort.Length - 1);
                }
                if (wort.Length > 5 && wort.EndsWith("s"))
                {
                    wort = wort.Substring(0, wort.Length - 1);
                }

                if (ergebnis != "")
                {
                    ergebnis += " ";
                }
                ergebnis += wort;
            }
            return ergebnis.Trim();
        }
        //chatgpt ende

        public static int BerechneAehnlichkeit(string nameA, string nameB)
        {
            string a = NormalisiereName(nameA);
            string b = NormalisiereName(nameB);
            if (a == "" || b == "")
            {
                return 0;
            }
            if (a == b)
            {
                return 1000;
            }
            if (a.Contains(b) || b.Contains(a))
            {
                return 800;
            }


            //chatgpt, promt: wie amche ich, dass die Ähnlichkeit von zwei Namen berechnet, indem sie die Anzahl der gemeinsamen Wörter zählt. Jedes gemeinsame Wort erhöht die Ähnlichkeit um 200 Punkte. Wenn ein Wort in einem Namen im anderen Namen enthalten ist (z.B. "Apfel" und "Apfelmus"), erhöht das die Ähnlichkeit um 120 Punkte. Wenn die Ähnlichkeit 0 ist, aber die ersten 4 Buchstaben der Namen gleich sind, soll die Ähnlichkeit 150 Punkte betragen.
            string[] aWoerter = a.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] bWoerter = b.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int score = 0;
            foreach (string wa in aWoerter)
            {

                foreach (string wb in bWoerter)
                {
             
                    if (wa == wb)
                    {
                        score += 200;
                    }
                    else if (wa.Contains(wb) || wb.Contains(wa))
                    {
                        score += 120;
                    }
                }
            }
            if (score == 0 && a.Length >= 4 && b.Length >= 4 &&
                a.Substring(0, 4) == b.Substring(0, 4))
            {
                score = 150;
            }
            return score;

            //chatgpt ende
        }
    }
}