using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PantryToPlate.Models
{
    public class Lebensmittel
    {
        public string Name { get; private set; }
        public double KalorienPro100g { get; private set; }
        public double ProteinePro100g { get; private set; }
        public double FettPro100g { get; private set; }
        public double KohlenhydratePro100g { get; private set; }
        public double BallaststoffePro100g { get; private set; }

        public Lebensmittel(string name, double kal, double pro, double fett, double kohlen, double ballast)
        {
            Name = name;
            KalorienPro100g = kal;
            ProteinePro100g = pro;
            FettPro100g = fett;
            KohlenhydratePro100g = kohlen;
            BallaststoffePro100g = ballast;
        }

        private static bool VersucheZahlZuLesen(string text, out double wert)
        {
            wert = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim().Replace(',', '.');
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out wert);
        }

        public static List<Lebensmittel> LadeAlleAusCsv(string dateiPfad = "data/test_utf8.csv")
        {
            List<Lebensmittel> liste = new List<Lebensmittel>();
            if (!File.Exists(dateiPfad))
            {
                return liste;
            }

            string[] zeilen = File.ReadAllLines(dateiPfad);

            for (int i = 1; i < zeilen.Length; i++)
            {
                string[] teile = zeilen[i].Split(';');

                if (teile.Length < 5)
                {
                    continue;
                }

                if (VersucheZahlZuLesen(teile[1], out double kal) && VersucheZahlZuLesen(teile[2], out double pro) && VersucheZahlZuLesen(teile[3], out double fett) && VersucheZahlZuLesen(teile[4], out double kohlen))
                {
                    double ballast = (teile.Length >= 6 && VersucheZahlZuLesen(teile[5], out double b)) ? b : 0;
                    liste.Add(new Lebensmittel(teile[0], kal, pro, fett, kohlen, ballast));
                }
            }
            return liste;
        }
    }
}
