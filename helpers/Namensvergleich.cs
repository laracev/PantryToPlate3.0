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

        public static string NameOhneMenge(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }
            int letzteKlammer = text.LastIndexOf('(');
            if (letzteKlammer > 0)
            {
                return text.Substring(0, letzteKlammer).Trim();
            }
            return text.Trim();
        }

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

        public static string NormalisiereName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }
            name = NameOhneMenge(name).ToLower().Trim();
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

        public static bool IstSinnvollesWort(string wort)
        {
            if (wort.Length <= 2)
            {
                return false;
            }
            bool nurZiffern = true;
            foreach (char c in wort)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',')
                {
                    nurZiffern = false;
                    break;
                }
            }
            return !nurZiffern;
        }

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

            string[] aWoerter = a.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] bWoerter = b.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int score = 0;
            foreach (string wa in aWoerter)
            {
                if (!IstSinnvollesWort(wa))
                {
                    continue;
                }
                foreach (string wb in bWoerter)
                {
                    if (!IstSinnvollesWort(wb))
                    {
                        continue;
                    }
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
        }
    }
}