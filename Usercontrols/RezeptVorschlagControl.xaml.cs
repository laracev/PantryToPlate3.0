using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PantryToPlate.Models;

namespace PantryToPlate.Usercontrols
{
    public partial class RezeptVorschlagControl : UserControl
    {
        private Rezept bestesRezept;

        public RezeptVorschlagControl()
        {
            InitializeComponent();
        }

        public void SetzeBestesRezept(Rezept rezept)
        {
            if (rezept == null)
            {
                txtRezeptName.Text = "Keine Rezepte verfügbar";
                txtMatchProzent.Text = "0%";
       
                btnZumRezept.IsEnabled = false;
                return;
            }

            bestesRezept = rezept;

            // Name kürzen wenn zu lang
            string name = rezept.Name;
            if (name.Length > 40)
            {
                name = name.Substring(0, 37) + "...";
            }
            txtRezeptName.Text = name;

            // Match Prozent mit Farbe
            txtMatchProzent.Text = rezept.MatchProzent + "%";

            if (rezept.MatchProzent >= 80)
            {
                matchBorder.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Grün
            }
            else if (rezept.MatchProzent >= 50)
            {
                matchBorder.Background = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Orange
            }
            else
            {
                matchBorder.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Rot
            }

            // Top 3 Zutaten anzeigen
            string zutatenText = "";
            int maxZutaten = 3;
            if (rezept.Zutaten.Count < maxZutaten)
            {
                maxZutaten = rezept.Zutaten.Count;
            }

            for (int i = 0; i < maxZutaten; i++)
            {
                if (i > 0)
                {
                    zutatenText = zutatenText + " • ";
                }
                zutatenText = zutatenText + rezept.Zutaten[i];
            }

            if (rezept.Zutaten.Count > 3)
            {
                zutatenText = zutatenText + " • +" + (rezept.Zutaten.Count - 3) + " mehr";
            }

         
            btnZumRezept.IsEnabled = true;
        }

        private void btnZumRezept_Click(object sender, RoutedEventArgs e)
        {
            if (bestesRezept != null)
            {
                RezepteWindow win = new RezepteWindow();
                win.ShowDialog();
            }
        }
    }
}