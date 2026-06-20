using System;

namespace PantryToPlate.logik
{
    public class KalorienzielRechner
    {
        public double BerechneKalorienziel(double gewicht, double groesse, int alter, string geschlecht, double aktivitaetsfaktor, string ziel)
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

            double gesamtumsatz = grundumsatz * aktivitaetsfaktor;

            if (ziel == "Abnehmen")
            {
                gesamtumsatz -= 200;
            }
            else if (ziel == "Zunehmen")
            {
                gesamtumsatz += 200;
            }

            if (gesamtumsatz < 1000)
            {
                gesamtumsatz = 1000;
            }

            return Math.Round(gesamtumsatz);
        }
    }
}
