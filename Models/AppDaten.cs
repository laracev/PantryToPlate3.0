using System.Collections.Generic;
using PantryToPlate.Models;

namespace PantryToPlate
{
    public static class AppDaten
    {
        public static List<Lebensmittel> Lebensmittel { get; private set; } = new List<Lebensmittel>();
        public static List<Rezept> Rezepte { get; private set; } = new List<Rezept>();
        public static List<PantryItem> Pantry { get; private set; } = new List<PantryItem>();
        public static List<MetEintrag> MetEintraege { get; private set; } = new List<MetEintrag>();
        public static Dictionary<string, double> LebensmittelKalorien { get; private set; } = new Dictionary<string, double>();
        public static bool IstGeladen { get; private set; }

        public static void SetzeLebensmittel(List<Lebensmittel> lebensmittel)
        {
            if (lebensmittel == null)
            {
                Lebensmittel = new List<Lebensmittel>();
            }
            else
            {
                Lebensmittel = lebensmittel;
            }
        }

        public static void SetzeRezepte(List<Rezept> rezepte)
        {
            if (rezepte == null)
            {
                Rezepte = new List<Rezept>();
            }
            else
            {
                Rezepte = rezepte;
            }
        }

        public static void SetzePantry(List<PantryItem> pantry)
        {
            if (pantry == null)
            {
                Pantry = new List<PantryItem>();
            }
            else
            {
                Pantry = pantry;
            }
        }

        public static void SetzeMetEintraege(List<MetEintrag> metEintraege)
        {
            if (metEintraege == null)
            {
                MetEintraege = new List<MetEintrag>();
            }
            else
            {
                MetEintraege = metEintraege;
            }
        }

        public static void SetzeGeladen(bool istGeladen)
        {
            IstGeladen = istGeladen;
        }
    }
}
