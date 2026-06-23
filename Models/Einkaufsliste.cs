using System.Collections.Generic;
using System.IO;

namespace PantryToPlate.Models
{
    public static class Einkaufsliste
    {
        public static List<string> Lade(string dateiPfad = "data/Einkaufsliste.csv")
        {
            List<string> eintraege = new List<string>();
            if (!File.Exists(dateiPfad))
            {
                return eintraege;
            }

            string[] zeilen = File.ReadAllLines(dateiPfad);

            for (int i = 1; i < zeilen.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(zeilen[i]))
                {
                    eintraege.Add(zeilen[i].Trim());
                }
            }
            return eintraege;
        }

        public static void Speichere(List<string> eintraege, string dateiPfad = "data/Einkaufsliste.csv")
        {
            _ = Directory.CreateDirectory("data");
            List<string> zeilen = new List<string> { "Zutat" };
            if (eintraege != null)
            {
                zeilen.AddRange(eintraege); // aus internet genommen, is kinda einfacher als andere stuff
            }
            File.WriteAllLines(dateiPfad, zeilen);
        }
    }
}