using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PantryToPlate.Models
{
    public class FitnessEintrag
    {
        public DateTime Datum { get; private set; } // hab ich durch recherche im internet gefunden 
        public string Aktivitaet { get; private set; }
        public double VerbrannteKalorien { get; private set; }

        public FitnessEintrag(DateTime datum, string aktivitaet, double kalorien)
        {
            Datum = datum;
            Aktivitaet = aktivitaet;
            VerbrannteKalorien = kalorien;
        }

        public static List<FitnessEintrag> LadeVonTag(DateTime datum, string dateiPfad = "data/FitnessEintraege.csv")
        {
            List<FitnessEintrag> liste = new List<FitnessEintrag>();

            if (!File.Exists(dateiPfad))
            {
                return liste;
            }

            string gesuchtesDatum = datum.ToString("yyyy-MM-dd");
            string[] zeilen = File.ReadAllLines(dateiPfad);

            for (int i = 1; i < zeilen.Length; i++)
            {
                string[] teile = zeilen[i].Split(';');

                if (teile.Length < 3 || teile[0] != gesuchtesDatum)
                {
                    continue;
                }

                if (double.TryParse(teile[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double kalorien))
                {
                    liste.Add(new FitnessEintrag(datum, teile[1], kalorien));
                }
            }

            return liste;
        }

        public static void Speichere(FitnessEintrag eintrag, string dateiPfad = "data/FitnessEintraege.csv")
        {
            Directory.CreateDirectory("data");

            string zeile = eintrag.Datum.ToString("yyyy-MM-dd") + ";" + eintrag.Aktivitaet + ";" + eintrag.VerbrannteKalorien.ToString(CultureInfo.InvariantCulture);

            if (!File.Exists(dateiPfad))
            {
                File.WriteAllText(dateiPfad, "Datum;Aktivitaet;Kalorien\n" + zeile);
            }
            else
            {
                File.AppendAllText(dateiPfad, "\n" + zeile);
            }
        }

        public static bool LoescheVonTagNachIndex(DateTime datum, int index, string dateiPfad = "data/FitnessEintraege.csv")
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
                neueZeilen.Add("Datum;Aktivitaet;Kalorien");
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


    // idee von dem programmierer kollge die klasse in dieselbe file zutun wie fitnesseintrag
    public class MetEintrag
    {
        public string Aktivitaet { get; private set; }
        public double MetWert { get; private set; }

        public MetEintrag(string aktivitaet, double metWert)
        {
            Aktivitaet = aktivitaet;
            MetWert = metWert;
        }

        public static List<MetEintrag> LadeAlleAusCsv(
            string dateiPfad = "data/MET_Werte_Tabelle.csv")
        {
            List<MetEintrag> liste = new List<MetEintrag>();

            if (!File.Exists(dateiPfad))
            {
                return liste;
            }

            string[] zeilen = File.ReadAllLines(dateiPfad);

            for (int i = 1; i < zeilen.Length; i++)
            {
                string[] teile = zeilen[i].Split(';');

                if (teile.Length >= 2 && double.TryParse(teile[1].Replace('.', ','), out double metWert))
                {
                    liste.Add(new MetEintrag(teile[0], metWert));
                }
            }

            return liste;
        }
    }
}
