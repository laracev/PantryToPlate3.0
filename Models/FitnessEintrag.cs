using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PantryToPlate.Models
{
    public class FitnessEintrag
    {
        public DateTime Datum { get; private set; }
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
            List<FitnessEintrag> list = new List<FitnessEintrag>();
            if (!File.Exists(dateiPfad))
            {
                return list;
            }
            string target = datum.ToString("yyyy-MM-dd");
            string[] zeilen = File.ReadAllLines(dateiPfad);
            for (int i = 1; i < zeilen.Length; i++)
            {
                string[] teile = zeilen[i].Split(';');
                if (teile.Length < 3)
                {
                    continue;
                }
                if (teile[0] != target)
                {
                    continue;
                }
                if (double.TryParse(teile[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double kal))
                {
                    list.Add(new FitnessEintrag(datum, teile[1], kal));
                }
            }
            return list;
        }

        public static void Speichere(FitnessEintrag eintrag, string dateiPfad = "data/FitnessEintraege.csv")
        {
            Directory.CreateDirectory("data");
            string zeile = eintrag.Datum.ToString("yyyy-MM-dd") + ";" + eintrag.Aktivitaet + ";" +
                eintrag.VerbrannteKalorien.ToString(CultureInfo.InvariantCulture);
            if (!File.Exists(dateiPfad))
            {
                File.WriteAllText(dateiPfad, "Datum;Aktivitaet;Kalorien\n" + zeile);
            }
            else
            {
                File.AppendAllText(dateiPfad, "\n" + zeile);
            }
        }
    }

    public class MetEintrag
    {
        public string Aktivitaet { get; private set; }
        public double MetWert { get; private set; }

        public MetEintrag(string aktivitaet, double met)
        {
            Aktivitaet = aktivitaet;
            MetWert = met;
        }

        public static List<MetEintrag> LadeAlleAusCsv(string dateiPfad = "data/MET_Werte_Tabelle.csv")
        {
            List<MetEintrag> list = new List<MetEintrag>();
            if (!File.Exists(dateiPfad))
            {
                return list;
            }
            string[] zeilen = File.ReadAllLines(dateiPfad);
            for (int i = 1; i < zeilen.Length; i++)
            {
                string[] teile = zeilen[i].Split(';');
                if (teile.Length >= 2 && double.TryParse(teile[1].Replace('.', ','), out double met))
                {
                    list.Add(new MetEintrag(teile[0], met));
                }
            }
            return list;
        }
    }
}