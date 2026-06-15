using PantryToPlate.Models;
using System.Collections.Generic;

namespace PantryToPlate
{
    public static class AppDaten
    {
        public static List<Lebensmittel> Lebensmittel = new List<Lebensmittel>();
        public static List<Rezept> Rezepte = new List<Rezept>();
        public static List<PantryItem> Pantry = new List<PantryItem>();
        public static List<MetEintrag> MetEintraege = new List<MetEintrag>();
        public static Dictionary<string, double> LebensmittelKalorien = new Dictionary<string, double>();
        public static bool IstGeladen = false;
    }
}