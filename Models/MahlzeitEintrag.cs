using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PantryToPlate.Models
{
    public class MahlzeitEintrag
    {
        public string Name { get; private set; }
        public double Menge { get; private set; }
        public double Kalorien { get; private set; }
        public double Proteine { get; private set; }
        public double Kohlenhydrate { get; private set; }
        public double Fett { get; private set; }

        public MahlzeitEintrag(string name, double menge, double kalorien, double proteine, double kohlenhydrate, double fett)
        {
            Name = name;
            Menge = menge;
            Kalorien = kalorien;
            Proteine = proteine;
            Kohlenhydrate = kohlenhydrate;
            Fett = fett;
        }

        public static List<MahlzeitEintrag> LadeVonTag(DateTime datum, string dateiPfad = "data/tagesmahlzeiten.csv")
        {
            List<MahlzeitEintrag> result = new List<MahlzeitEintrag>();

            if (!File.Exists(dateiPfad))
            {
                return result;
            }

            string gesuchtesDatum = datum.ToString("yyyy-MM-dd");
            string[] zeilen = File.ReadAllLines(dateiPfad);

            for (int i = 1; i < zeilen.Length; i++)
            {
                string[] teile = zeilen[i].Split(';');

                if (teile.Length < 7 || teile[0] != gesuchtesDatum)
                {
                    continue;
                }

                if (double.TryParse(teile[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double menge) && double.TryParse(teile[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double kalorien) && double.TryParse(teile[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double proteine) && double.TryParse(teile[5], NumberStyles.Any, CultureInfo.InvariantCulture, out double kohlenhydrate) && double.TryParse(teile[6], NumberStyles.Any, CultureInfo.InvariantCulture, out double fett))
                {
                    result.Add(new MahlzeitEintrag(teile[1], menge, kalorien, proteine, kohlenhydrate, fett));
                }
            }

            return result;
        }

        public static void Speichere(MahlzeitEintrag eintrag, string dateiPfad = "data/tagesmahlzeiten.csv")
        {
            Directory.CreateDirectory("data");

            string zeile = DateTime.Now.ToString("yyyy-MM-dd") + ";" + eintrag.Name + ";" + eintrag.Menge.ToString(CultureInfo.InvariantCulture) + ";" + eintrag.Kalorien.ToString(CultureInfo.InvariantCulture) + ";" + eintrag.Proteine.ToString(CultureInfo.InvariantCulture) + ";" + eintrag.Kohlenhydrate.ToString(CultureInfo.InvariantCulture) + ";" + eintrag.Fett.ToString(CultureInfo.InvariantCulture);

            if (!File.Exists(dateiPfad))
            {
                File.WriteAllText(dateiPfad, "Datum;Lebensmittel;Gramm;Kalorien;Proteine;Kohlenhydrate;Fett\n" + zeile);
            }
            else
            {
                File.AppendAllText(dateiPfad, "\n" + zeile);
            }
        }

        public static bool LoescheVonTagNachIndex(DateTime datum, int index, string dateiPfad = "data/tagesmahlzeiten.csv")
        {
            if (!File.Exists(dateiPfad) || index < 0)
            {
                return false;
            }

            string[] zeilen = File.ReadAllLines(dateiPfad);
            List<string> neueZeilen = new List<string>();
            string gesuchtesDatum = datum.ToString("yyyy-MM-dd");
            int aktuellerIndex = 0;
            bool wurdeGeloescht = false;

            if (zeilen.Length > 0)
            {
                neueZeilen.Add(zeilen[0]);
            }
            else
            {
                neueZeilen.Add("Datum;Lebensmittel;Gramm;Kalorien;Proteine;Kohlenhydrate;Fett");
            }

            for (int i = 1; i < zeilen.Length; i++)
            {
                string[] teile = zeilen[i].Split(';');

                if (teile.Length > 0 && teile[0] == gesuchtesDatum)
                {
                    if (aktuellerIndex == index)
                    {
                        wurdeGeloescht = true;
                        aktuellerIndex++;
                        continue;
                    }

                    aktuellerIndex++;
                }

                neueZeilen.Add(zeilen[i]);
            }

            if (wurdeGeloescht)
            {
                File.WriteAllLines(dateiPfad, neueZeilen);
            }

            return wurdeGeloescht;
        }
    }
}
