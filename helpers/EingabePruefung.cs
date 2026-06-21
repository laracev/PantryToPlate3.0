namespace PantryToPlate.helpers
{
    public static class EingabePruefung
    {
        public static bool IstGueltigesGewicht(string text)
        {
            return VersucheGewichtZuLesen(text, out _);
        }

        public static bool IstGueltigeGroesse(string text)
        {
            return VersucheGroesseZuLesen(text, out _);
        }

        public static bool IstGueltigesAlter(string text)
        {
            return VersucheAlterZuLesen(text, out _);
        }

        public static bool IstGueltigeMenge(string text)
        {
            return VersucheMengeZuLesen(text, out _);
        }

        public static bool IstGueltigeDauer(string text)
        {
            return VersucheDauerZuLesen(text, out _);
        }

        public static bool VersucheGewichtZuLesen(string text, out double gewicht)
        {
            gewicht = 0;
            if (!double.TryParse(text, out double wert) || wert < 20 || wert > 500)
            {
                return false;
            }
            gewicht = wert;
            return true;
        }

        public static bool VersucheGroesseZuLesen(string text, out double groesse)
        {
            groesse = 0;
            if (!double.TryParse(text, out double wert) || wert < 100 || wert > 250)
            {
                return false;
            }
            groesse = wert;
            return true;
        }

        public static bool VersucheAlterZuLesen(string text, out int alter)
        {
            alter = 0;
            if (!int.TryParse(text, out int wert) || wert < 14 || wert > 120)
            {
                return false;
            }
            alter = wert;
            return true;
        }

        public static bool VersucheMengeZuLesen(string text, out double menge)
        {
            menge = 0;
            if (!double.TryParse(text, out double wert) || wert <= 0 || wert > 100000)
            {
                return false;
            }
            menge = wert;
            return true;
        }

        public static bool VersucheDauerZuLesen(string text, out double dauer)
        {
            dauer = 0;
            if (!double.TryParse(text, out double wert) || wert <= 0 || wert > 1440)
            {
                return false;
            }
            dauer = wert;
            return true;
        }

        public static bool IstAuswahlKorrekt(object auswahl)
        {
            return auswahl != null;
        }
    }
}
