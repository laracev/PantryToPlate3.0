using System.Collections.Generic;
using System.IO;

namespace PantryToPlate.Models
{
    public class Rezept
    {
        public string Name { get; private set; }
        public double KalorienProPortion { get; private set; }
        public List<string> Zutaten { get; private set; }
        public List<double> ZutatenMengen { get; private set; }
        public int MatchProzent { get; private set; }
        public string Anleitung { get; private set; }

        public Rezept(string name, string anleitung, List<string> zutaten, List<double> mengen)
        {
            Name = name;
            Anleitung = anleitung;
            Zutaten = zutaten;
            ZutatenMengen = mengen;
        }

        public void SetzeKalorienProPortion(double kalorien)
        {
            KalorienProPortion = kalorien < 0 ? 0 : kalorien;
        }

        public void SetzeMatchProzent(int prozent)
        {
            if (prozent < 0)
            {
                MatchProzent = 0;
            }
            else if (prozent > 100)
            {
                MatchProzent = 100;
            }
            else
            {
                MatchProzent = prozent;
            }
        }

        public static List<Rezept> LadeAlleAusCsv(string dateiPfad = "data/rezepte.csv")
        {
            List<Rezept> rezepteListe = new List<Rezept>();
            if (!File.Exists(dateiPfad))
            {
                return rezepteListe;
            }

            string[] zeilen = File.ReadAllLines(dateiPfad);
            for (int i = 1; i < zeilen.Length; i++)
            {
                Rezept rezept = ParseZeile(zeilen[i]);
                if (rezept != null)
                {
                    rezepteListe.Add(rezept);
                }
            }
            return rezepteListe;
        }

        private static Rezept ParseZeile(string zeile)
        {
            if (string.IsNullOrWhiteSpace(zeile))
            {
                return null;
            }

            string[] teile = zeile.Split(';');
            if (teile.Length < 3 || string.IsNullOrWhiteSpace(teile[0]))
            {
                return null;
            }

            string name = teile[0].Trim();
            string anleitung = teile.Length >= 4 ? teile[3] : "Keine Anleitung verfügbar.";
            anleitung = anleitung.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<p>", "").Replace("</p>", "");

            List<string> zutatenListe = new List<string>();
            List<double> mengenListe = new List<double>();
            string[] zutatenRoh = teile[2].Split('|');

            foreach (string zutatBlock in zutatenRoh)
            {
                int doppelpunkt = zutatBlock.LastIndexOf(':');
                if (doppelpunkt <= 0)
                {
                    continue;
                }

                string zutatName = zutatBlock.Substring(0, doppelpunkt).Trim();
                string mengeText = zutatBlock.Substring(doppelpunkt + 1).Trim().Replace('.', ',');

                if (double.TryParse(mengeText, out double menge) && menge > 0 && !string.IsNullOrWhiteSpace(zutatName))
                {
                    zutatenListe.Add(zutatName);
                    mengenListe.Add(menge);
                }
            }

            if (zutatenListe.Count == 0 || zutatenListe.Count != mengenListe.Count)
            {
                return null;
            }

            return new Rezept(name, anleitung, zutatenListe, mengenListe);
        }
    }
}
