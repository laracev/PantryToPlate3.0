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

            //chatgpt start: promt: Kürze den Rezeptnamen auf maximal 40 Zeichen und hilf mir diese matching prozent anzeigen zu können
            string name = rezept.Name;
            if (name.Length > 40)
            {
                name = name.Substring(0, 37) + "...";
            }
            txtRezeptName.Text = name;

            txtMatchProzent.Text = rezept.MatchProzent + "%";

            if (rezept.MatchProzent >= 80)
            {
                matchBorder.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129)); //bin sehr stolz drauf das ich das rausgefunden hab lol ich bin voll gut mit rgb vom üben mit leds einstellen
            }
            else if (rezept.MatchProzent >= 50)
            {
                matchBorder.Background = new SolidColorBrush(Color.FromRgb(245, 158, 11));
            }
            else
            {
                matchBorder.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            }


            btnZumRezept.IsEnabled = true;
        }
        //chatgpt end

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