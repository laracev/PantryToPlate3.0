using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.Generic;

namespace PantryToPlate.Models
{
    public class Rezept
    {
        public string Name { get; set; }
        public double KalorienProPortion { get; set; }
        public List<string> Zutaten { get; set; }
        public List<double> ZutatenMengen { get; set; }
        public double MatchProzent { get; set; } // Für Sortierung
        public int FehlendeZutaten { get; set; }
        public string Anleitung { get; set; }

        public Rezept()
        {
            Zutaten = new List<string>();
            ZutatenMengen = new List<double>();
        }
    }
}
