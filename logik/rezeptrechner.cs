using PantryToPlate.helpers;
using PantryToPlate.Models;
using System.Collections.Generic;

namespace PantryToPlate.logik
{
    public class RezeptRechner
    {
        public double BerechneKalorien(Rezept rezept, Dictionary<string, double> kalorienTabelle)
        {
            double gesamt = 0;
            for (int i = 0; i < rezept.Zutaten.Count; i++)
            {
                string zutat = rezept.Zutaten[i].ToLower();
                double kalorienPro100g = 100;
                if (kalorienTabelle.ContainsKey(zutat))
                {
                    kalorienPro100g = kalorienTabelle[zutat];
                }
                else
                {
                    foreach (string name in kalorienTabelle.Keys)
                    {
                        if (name.Contains(zutat) || zutat.Contains(name))
                        {
                            kalorienPro100g = kalorienTabelle[name];
                            break;
                        }
                    }
                }
                double menge = rezept.ZutatenMengen[i];
                gesamt = gesamt + kalorienPro100g * menge / 100;
            }
            return gesamt;
        }

        public int BerechneMatch(Rezept rezept, List<PantryItem> pantryItems)
        {
            int vorhanden = 0;
            for (int i = 0; i < rezept.Zutaten.Count; i++)
            {
                bool gefunden = false;
                string zutat = rezept.Zutaten[i];
                double benoetigteMenge = rezept.ZutatenMengen[i];
                for (int j = 0; j < pantryItems.Count; j++)
                {
                    if (Namensvergleich.BerechneAehnlichkeit(zutat, pantryItems[j].Name) >= 120 &&
                        pantryItems[j].Menge >= benoetigteMenge)
                    {
                        gefunden = true;
                        break;
                    }
                }
                if (gefunden)
                {
                    vorhanden++;
                }
            }
            if (rezept.Zutaten.Count == 0)
            {
                return 0;
            }
            return vorhanden * 100 / rezept.Zutaten.Count;
        }


        //chatgt: wie mache ich so berechnen von den fehlenden zutaten am besten?nn
        public List<string> ErmittleFehlendeZutaten(Rezept rezept, List<PantryItem> pantryItems)
        {
            List<string> fehlende = new List<string>();
            for (int i = 0; i < rezept.Zutaten.Count; i++)
            {
                bool gefunden = false;
                string zutat = rezept.Zutaten[i];
                double benoetigt = rezept.ZutatenMengen[i];
                for (int j = 0; j < pantryItems.Count; j++)
                {
                    if (Namensvergleich.BerechneAehnlichkeit(zutat, pantryItems[j].Name) >= 120 &&
                        pantryItems[j].Menge >= benoetigt)
                    {
                        gefunden = true;
                        break;
                    }
                }
                if (!gefunden)
                {
                    fehlende.Add(zutat);
                }
            }
            return fehlende;
            //chatgpt ende
        }
    }
}