using PantryToPlate.Models;
using System.Collections.Generic;

namespace PantryToPlate.logik
{
    public class KalorienRechner
    {
        public double BerechneGegesseneKalorien(List<MahlzeitEintrag> mahlzeiten)
        {
            double gesamt = 0;
            for (int i = 0; i < mahlzeiten.Count; i++)
            {
                gesamt = gesamt + mahlzeiten[i].Kalorien;
            }
            return gesamt;
        }

        public double BerechneNettoKalorien(double gegessen, double verbrannt)
        {
            double netto = gegessen - verbrannt;
            if (netto < 0)
            {
                netto = 0;
            }
            return netto;
        }

        public double BerechneUebrigeKalorien(double kalorienZiel, double nettoKalorien)
        {
            double uebrig = kalorienZiel - nettoKalorien;
            if (uebrig < 0)
            {
                uebrig = 0;
            }
            return uebrig;
        }


        public double BerechneUeberschussKalorien(double kalorienZiel, double nettoKalorien)
        {
            double ueberschuss = nettoKalorien - kalorienZiel;
            if (ueberschuss < 0)
            {
                ueberschuss = 0;
            }
            return ueberschuss;
        }

        public double BerechneProteine(List<MahlzeitEintrag> mahlzeiten)
        {
            double gesamt = 0;
            for (int i = 0; i < mahlzeiten.Count; i++)
            {
                gesamt = gesamt + mahlzeiten[i].Proteine;
            }
            return gesamt;
        }

        public double BerechneKohlenhydrate(List<MahlzeitEintrag> mahlzeiten)
        {
            double gesamt = 0;
            for (int i = 0; i < mahlzeiten.Count; i++)
            {
                gesamt = gesamt + mahlzeiten[i].Kohlenhydrate;
            }
            return gesamt;
        }

        public double BerechneFett(List<MahlzeitEintrag> mahlzeiten)
        {
            double gesamt = 0;
            for (int i = 0; i < mahlzeiten.Count; i++)
            {
                gesamt = gesamt + mahlzeiten[i].Fett;
            }
            return gesamt;
        }
    }
}