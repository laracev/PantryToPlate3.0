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
            Lebensmittel = lebensmittel ?? new List<Lebensmittel>(); //wenn lebensmittel nd null ist, nimm lebensmittel lul
        }

        public static void SetzeRezepte(List<Rezept> rezepte)
        {
            Rezepte = rezepte ?? new List<Rezept>();
        }

        public static void SetzePantry(List<PantryItem> pantry)
        {
            Pantry = pantry ?? new List<PantryItem>();
        }

        public static void SetzeMetEintraege(List<MetEintrag> metEintraege)
        {
            MetEintraege = metEintraege ?? new List<MetEintrag>();
        }

        public static void SetzeGeladen(bool istGeladen)
        {
            IstGeladen = istGeladen;
        }
    }
}
