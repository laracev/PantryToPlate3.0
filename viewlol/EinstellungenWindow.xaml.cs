using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PantryToPlate
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            LadeVorhandeneDaten();
        }
        private string benutzerDatei = "data/benutzer.csv";
        private double berechnetesZiel = 2000;


     private void LadeVorhandeneDaten()
        {
            if (File.Exists(benutzerDatei))
            {
                string[] zeilen = File.ReadAllLines(benutzerDatei);
                if (zeilen.Length > 1)
                {
                    string[] teile = zeilen[1].Split(';');
                    if (teile.Length >= 7)
                    {
                        txtGewicht.Text = teile[1];
                        txtGroesse.Text = teile[2];
                        txtAlter.Text = teile[3];
                        
                        if (teile[4] == "Weiblich")
                            cboGeschlecht.SelectedIndex = 0;
                        else
                            cboGeschlecht.SelectedIndex = 1;
                        
                        for (int i = 0; i < cboAktivitaet.Items.Count; i++)
                        {
                            ComboBoxItem item = (ComboBoxItem)cboAktivitaet.Items[i];
                            if (item.Content.ToString() == teile[5])
                            {
                                cboAktivitaet.SelectedIndex = i;
                                break;
                            }
                        }
                        
                        for (int i = 0; i < cboZiel.Items.Count; i++)
                        {
                            ComboBoxItem item = (ComboBoxItem)cboZiel.Items[i];
                            if (item.Content.ToString() == teile[6])
                            {
                                cboZiel.SelectedIndex = i;
                                break;
                            }
                        }
                        
                        double.TryParse(teile[0], out berechnetesZiel);
                        txtKalorienZiel.Text = berechnetesZiel.ToString("0");
                    }
                }
            }
            
            if (cboAktivitaet.SelectedIndex < 0) cboAktivitaet.SelectedIndex = 1;
            if (cboZiel.SelectedIndex < 0) cboZiel.SelectedIndex = 1;
        }

        private void btnBerechnen_Click(object sender, RoutedEventArgs e)
        {
            double gewicht, groesse;
            int alter;
            
            if (!double.TryParse(txtGewicht.Text, out gewicht))
            {
                MessageBox.Show("Bitte gültiges Gewicht eingeben");
                return;
            }
            
            if (!double.TryParse(txtGroesse.Text, out groesse))
            {
                MessageBox.Show("Bitte gültige Größe eingeben");
                return;
            }
            
            if (!int.TryParse(txtAlter.Text, out alter))
            {
                MessageBox.Show("Bitte gültiges Alter eingeben");
                return;
            }
            
            string geschlecht = ((ComboBoxItem)cboGeschlecht.SelectedItem).Content.ToString();
            string aktivitaet = ((ComboBoxItem)cboAktivitaet.SelectedItem).Content.ToString();
            string ziel = ((ComboBoxItem)cboZiel.SelectedItem).Content.ToString();
            
            double grundumsatz = 0;
            
            if (geschlecht == "Weiblich")
            {
                grundumsatz = 655 + (9.6 * gewicht) + (1.8 * groesse) - (4.7 * alter);
            }
            else
            {
                grundumsatz = 66 + (13.7 * gewicht) + (5 * groesse) - (6.8 * alter);
            }
            
            double faktor = 1.2;
            if (aktivitaet.Contains("1.375")) faktor = 1.375;
            if (aktivitaet.Contains("1.55")) faktor = 1.55;
            if (aktivitaet.Contains("1.725")) faktor = 1.725;
            
            double gesamtumsatz = grundumsatz * faktor;
            
            double anpassung = 0;
            if (ziel.Contains("-500")) anpassung = -500;
            if (ziel.Contains("+500")) anpassung = 500;
            
            berechnetesZiel = gesamtumsatz + anpassung;
            if (berechnetesZiel < 1200) berechnetesZiel = 1200;
            
            txtKalorienZiel.Text = berechnetesZiel.ToString("0");
        }

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            if (berechnetesZiel <= 0)
            {
                MessageBox.Show("Bitte zuerst berechnen");
                return;
            }
            
            string gewicht = txtGewicht.Text;
            string groesse = txtGroesse.Text;
            string alter = txtAlter.Text;
            string geschlecht = ((ComboBoxItem)cboGeschlecht.SelectedItem).Content.ToString();
            string aktivitaet = ((ComboBoxItem)cboAktivitaet.SelectedItem).Content.ToString();
            string ziel = ((ComboBoxItem)cboZiel.SelectedItem).Content.ToString();
            
            string inhalt = "Kalorienziel;Gewicht;Groesse;Alter;Geschlecht;Aktivitaet;Ziel\n";
            inhalt += berechnetesZiel + ";" + gewicht + ";" + groesse + ";" + alter + ";" + geschlecht + ";" + aktivitaet + ";" + ziel;
            
            Directory.CreateDirectory("data");
            File.WriteAllText(benutzerDatei, inhalt);
            
            MessageBox.Show("Einstellungen gespeichert");
            this.Close();
        }

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
