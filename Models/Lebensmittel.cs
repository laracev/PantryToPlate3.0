using System;
using System.Collections.Generic;
using System.Text;

namespace PantryToPlate.Models
{
        public class Lebensmittel
        {
            public string Name { get; set; }
            public double KalorienPro100g { get; set; }
            public double ProteinePro100g { get; set; }
            public double FettPro100g { get; set; }
            public double KohlenhydratePro100g { get; set; }
            public double BallaststoffePro100g { get; set; }
        }
    }