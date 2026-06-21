using PantryToPlate.helpers;
using PantryToPlate.Models;
using System.Collections.Generic;

namespace PantryToPlate.logik
{
    public class RezeptRechner
    {

        //chatgpt start, promt: wie kann ich berechnen wie viele kalorien der user durch ein rezept aufgenommen hat?
        public double BerechneKalorien(Rezept rezept, Dictionary<string, double> kalorienTabelle)
        {
            double gesamt = 0;

            for (int i = 0; i < rezept.Zutaten.Count; i++)
            {
                string zutat = rezept.Zutaten[i].ToLower();
                double kalorienPro100g = 0;
                bool gefunden = false;

                if (kalorienTabelle.ContainsKey(zutat))
                {
                    kalorienPro100g = kalorienTabelle[zutat];
                    gefunden = true;
                }
                else
                {
                    foreach (string name in kalorienTabelle.Keys)
                    {
                        if (name.Contains(zutat) || zutat.Contains(name))
                        {
                            kalorienPro100g = kalorienTabelle[name];
                            gefunden = true;
                            break;
                        }
                    }
                }

                if (!gefunden)
                {
                    continue;
                }

                double menge = rezept.ZutatenMengen[i];
                gesamt += kalorienPro100g * menge / 100;
            }

            return gesamt;
        }
        //chatgpt ende
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
                    if (Namensvergleich.BerechneAehnlichkeit(zutat, pantryItems[j].Name) >= 120 && pantryItems[j].Menge >= benoetigteMenge)
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
                    if (Namensvergleich.BerechneAehnlichkeit(zutat, pantryItems[j].Name) >= 120 && pantryItems[j].Menge >= benoetigt)
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
        }

        public void AktualisiereMatches(List<Rezept> rezepte, List<PantryItem> pantryItems)
        {
            foreach (Rezept rezept in rezepte)
            {
                int match = BerechneMatch(rezept, pantryItems);
                rezept.SetzeMatchProzent(match);
            }
        }
    }
}
