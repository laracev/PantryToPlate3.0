using System.Globalization;
using System.IO;

namespace PantryToPlate.Models
{
    public class Benutzer
    {
        public double KalorienZiel { get; private set; }
        public double Gewicht { get; private set; }
        public double Groesse { get; private set; }
        public int Alter { get; private set; }
        public string Geschlecht { get; private set; }
        public string Aktivitaetslevel { get; private set; }
        public string Ziel { get; private set; }

        public Benutzer(double kalorienZiel, double gewicht, double groesse, int alter,
                        string geschlecht, string aktivitaet, string ziel)
        {
            KalorienZiel = kalorienZiel;
            Gewicht = gewicht;
            Groesse = groesse;
            Alter = alter;
            Geschlecht = geschlecht;
            Aktivitaetslevel = aktivitaet;
            Ziel = ziel;
        }

        public static Benutzer LadeAusCsv(string dateiPfad = "data/benutzer.csv")
        {
            if (!File.Exists(dateiPfad))
            {
                return null;
            }
            string[] zeilen = File.ReadAllLines(dateiPfad);
            if (zeilen.Length < 2)
            {
                return null;
            }
            string[] teile = zeilen[1].Split(';');
            if (teile.Length < 7)
            {
                return null;
            }

            if (double.TryParse(teile[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double ziel) &&
                double.TryParse(teile[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double gewicht) &&
                double.TryParse(teile[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double groesse) &&
                int.TryParse(teile[3], out int alter))
            {
                return new Benutzer(ziel, gewicht, groesse, alter, teile[4], teile[5], teile[6]);
            }
            return null;
        }

        public void Speichere(string dateiPfad = "data/benutzer.csv")
        {
            Directory.CreateDirectory("data");
            string inhalt = "Kalorienziel;Gewicht;Groesse;Alter;Geschlecht;Aktivitaet;Ziel\n" + KalorienZiel.ToString(CultureInfo.InvariantCulture) + ";" + Gewicht.ToString(CultureInfo.InvariantCulture) + ";" + Groesse.ToString(CultureInfo.InvariantCulture) + ";" + Alter + ";" + Geschlecht + ";" + Aktivitaetslevel + ";" + Ziel;
            File.WriteAllText(dateiPfad, inhalt);
        }
    }
}