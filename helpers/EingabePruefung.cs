using System;

namespace PantryToPlate.helpers
{
    static class EingabePruefung
    {
        public static bool IstGueltigesGewicht(string text)
        {
            double gewicht;

  
            if (!double.TryParse(text, out gewicht))
            {
                return false;
            }
            if (gewicht < 20 || gewicht > 500)
            {
                return false;
            }
            return true;
        }

        public static bool IstGueltigeGroesse(string text)
        {
            double groesse;
            if (!double.TryParse(text, out groesse))
            {
                return false;
            }
            if (groesse < 100 || groesse > 250)
            {
                return false;
            }
            return true;
        }

        public static bool IstGueltigesAlter(string text)
        {
            int alter;
            if (!int.TryParse(text, out alter))
            {
                return false;
            }
            if (alter < 14 || alter > 120)
            {
                return false;
            }
            return true;
        }

        public static bool IstGueltigeMenge(string text)
        {
            double menge;
            if (!double.TryParse(text, out menge))
            {
                return false;
            }
            if (menge <= 0)
            {
                return false;
            }
            return true;
        }

        public static bool IstGueltigeDauer(string text)
        {
            double dauer;
            if (!double.TryParse(text, out dauer))
            {
                return false;
            }
            if (dauer <= 0 || dauer > 1440)
            {
                return false;
            }
            return true;
        }

        public static bool IstAuswahlKorrekt(object auswahl)
        {
            return auswahl != null;
        }
    }
}