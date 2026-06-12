using System;
using System.Collections.Generic;
using System.Text;

namespace PantryToPlate.Models
{
    public class Benutzer
    {
        public double KalorienZiel { get; set; }
        public double Gewicht { get; set; }
        public double Groesse { get; set; }
        public int Alter { get; set; }
        public string Geschlecht { get; set; }
        public string Aktivitaetslevel { get; set; }
        public string Ziel { get; set; }
    }
}