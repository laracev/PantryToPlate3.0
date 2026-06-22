using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PantryToPlate.Models
{
    public class PantryItem
    {
        public string Name { get; private set; }
        public double Menge { get; private set; }

        //chatgpt start, promt: bitte mach das es so name erkennt ohne probleme bitti

        public PantryItem(string name, double menge)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                AppLogger.Info("Ungueltiger Pantry-Eintrag: leerer Name.");
                throw new ArgumentException("Der Name darf nicht leer sein.", nameof(name));
            }
            if (menge < 0)
            {
                AppLogger.Info("Ungueltiger Pantry-Eintrag: negative Menge.");
                throw new ArgumentOutOfRangeException(nameof(menge), "Die Menge darf nicht negativ sein.");
            }

            Name = name.Trim();
            Menge = menge;
        }
        //chatgpt ende
        public void ErhoeheMenge(double menge)
        {
            if (menge > 0)
            {
                Menge += menge;
            }
        }

        public bool VerringereMenge(double menge)
        {
            if (menge <= 0 || menge > Menge)
            {
                return false;
            }

            Menge -= menge;
            return true;
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
                if (teile.Length >= 2 && double.TryParse(teile[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double menge) && menge >= 0 && !string.IsNullOrWhiteSpace(teile[0]))
                {
                    try
                    {
                        liste.Add(new PantryItem(teile[0], menge));
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error(ex, "Ungueltiger Pantry-Eintrag wurde uebersprungen.");
                    }
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
