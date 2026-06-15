using System;

namespace PantryToPlate.logik
{
    class KalorienzielRechner
    {
        public double BerechneKalorienziel(double gewicht, double groesse, int alter, string geschlecht, string aktivitaet, string ziel)
        {
            double grundumsatz;
            if (geschlecht == "Weiblich")
            {
                grundumsatz = 655 + 9.6 * gewicht + 1.8 * groesse - 4.7 * alter;
            }
            else
            {
                grundumsatz = 66 + 13.7 * gewicht + 5 * groesse - 6.8 * alter;
            }

            double faktor = 1.2;
            if (aktivitaet.Contains("1.375"))
            {
                faktor = 1.375;
            }
            else if (aktivitaet.Contains("1.55"))
            {
                faktor = 1.55;
            }
            else if (aktivitaet.Contains("1.725"))
            {
                faktor = 1.725;
            }

            double gesamtumsatz = grundumsatz * faktor;
            if (ziel.Contains("abnehmen"))
            {
                gesamtumsatz = gesamtumsatz - 500;
            }
            else if (ziel.Contains("zunehmen"))
            {
                gesamtumsatz = gesamtumsatz + 500;
            }
            return Math.Round(gesamtumsatz);
        }
    }
}