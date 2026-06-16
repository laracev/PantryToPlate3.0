
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PantryToPlate.Models
{
    public class PantryItem
    {
        public string Name { get; set; }
        public double Menge { get; set; }  

        public PantryItem(string name, double menge)
        {
            Name = name;
            Menge = menge;
        }

        public static List<PantryItem> LadeAlleAusCsv(string dateiPfad = "data/pantry.csv")
        {
            List<PantryItem> liste = new List<PantryItem>();
            if (!File.Exists(dateiPfad))
            {
                return liste;
            }
            string[] zeilen = File.ReadAllLines(dateiPfad);
            for (int i = 1; i < zeilen.Length; i++)
            {
                string[] teile = zeilen[i].Split(';');
                if (teile.Length >= 2 && double.TryParse(teile[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double menge))
                {
                    liste.Add(new PantryItem(teile[0], menge));
                }
            }
            return liste;
        }

        public static void SpeichereAlle(List<PantryItem> items, string dateiPfad = "data/pantry.csv")
        {
            Directory.CreateDirectory("data");
            List<string> zeilen = new List<string> { "Name;Menge" };
            foreach (PantryItem item in items)
            {
                if (item.Menge > 0)
                {
                    zeilen.Add(item.Name + ";" + item.Menge.ToString(CultureInfo.InvariantCulture));
                }
            }
            File.WriteAllLines(dateiPfad, zeilen);
        }
    }
}