using PantryToPlate.helpers;
using PantryToPlate.logik;
using PantryToPlate.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PantryToPlate
{
    public partial class Window1 : Window
    {
        private double berechnetesZiel = 2000;
        private KalorienzielRechner kalorienzielRechner = new KalorienzielRechner();

        public Window1()
        {
            InitializeComponent();
            LadeVorhandeneDaten();
        }

        private void LadeVorhandeneDaten()
        {
            try
            {
                Benutzer benutzer = Benutzer.LadeAusCsv();
                if (benutzer != null)
                {
                    txtGewicht.Text = benutzer.Gewicht.ToString();
                    txtGroesse.Text = benutzer.Groesse.ToString();
                    txtAlter.Text = benutzer.Alter.ToString();
                    cboGeschlecht.SelectedIndex = (benutzer.Geschlecht == "Weiblich") ? 0 : 1;
                    for (int i = 0; i < cboAktivitaet.Items.Count; i++)
                    {
                        ComboBoxItem item = (ComboBoxItem)cboAktivitaet.Items[i];
                        if (item.Content.ToString() == benutzer.Aktivitaetslevel)
                        {
                            cboAktivitaet.SelectedIndex = i;
                            break;
                        }
                    }
                    for (int i = 0; i < cboZiel.Items.Count; i++)
                    {
                        ComboBoxItem item = (ComboBoxItem)cboZiel.Items[i];
                        if (item.Content.ToString() == benutzer.Ziel)
                        {
                            cboZiel.SelectedIndex = i;
                            break;
                        }
                    }
                    berechnetesZiel = benutzer.KalorienZiel;
                    txtKalorienZiel.Text = berechnetesZiel.ToString("0");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Laden der Benutzerdaten");
            }
            if (cboAktivitaet.SelectedIndex < 0)
            {
                cboAktivitaet.SelectedIndex = 1;
            }
            if (cboZiel.SelectedIndex < 0)
            {
                cboZiel.SelectedIndex = 1;
            }
        }

        private void btnBerechnen_Click(object sender, RoutedEventArgs e)
        {
            if (!EingabePruefung.IstGueltigesGewicht(txtGewicht.Text))
            {
                MessageBox.Show("Bitte ein gültiges Gewicht zwischen 20 und 500 kg eingeben.");
                return;
            }
            if (!EingabePruefung.IstGueltigeGroesse(txtGroesse.Text))
            {
                MessageBox.Show("Bitte eine gültige Größe zwischen 100 und 250 cm eingeben.");
                return;
            }
            if (!EingabePruefung.IstGueltigesAlter(txtAlter.Text))
            {
                MessageBox.Show("Bitte ein gültiges Alter zwischen 14 und 120 Jahren eingeben.");
                return;
            }
            if (cboGeschlecht.SelectedItem == null || cboAktivitaet.SelectedItem == null || cboZiel.SelectedItem == null)
            {
                MessageBox.Show("Bitte alle Felder auswählen.");
                return;
            }

            if (!double.TryParse(txtGewicht.Text, out double gewicht))
            {
                MessageBox.Show("Gewicht ungültig");
                return;
            }
            if (!double.TryParse(txtGroesse.Text, out double groesse))
            {
                MessageBox.Show("Größe ungültig");
                return;
            }
            if (!int.TryParse(txtAlter.Text, out int alter))
            {
                MessageBox.Show("Alter ungültig");
                return;
            }

            string geschlecht = ((ComboBoxItem)cboGeschlecht.SelectedItem).Content.ToString();
            string aktivitaet = ((ComboBoxItem)cboAktivitaet.SelectedItem).Content.ToString();
            string ziel = ((ComboBoxItem)cboZiel.SelectedItem).Content.ToString();

            berechnetesZiel = kalorienzielRechner.BerechneKalorienziel(gewicht, groesse, alter, geschlecht, aktivitaet, ziel);
            txtKalorienZiel.Text = berechnetesZiel.ToString("0");
        }

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            if (berechnetesZiel <= 0)
            {
                MessageBox.Show("Bitte zuerst berechnen.");
                return;
            }
            if (cboGeschlecht.SelectedItem == null || cboAktivitaet.SelectedItem == null || cboZiel.SelectedItem == null)
            {
                MessageBox.Show("Bitte alle Felder auswählen.");
                return;
            }

            if (!double.TryParse(txtGewicht.Text, out double gewicht))
            {
                MessageBox.Show("Gewicht ungültig");
                return;
            }
            if (!double.TryParse(txtGroesse.Text, out double groesse))
            {
                MessageBox.Show("Größe ungültig");
                return;
            }
            if (!int.TryParse(txtAlter.Text, out int alter))
            {
                MessageBox.Show("Alter ungültig");
                return;
            }

            string geschlecht = ((ComboBoxItem)cboGeschlecht.SelectedItem).Content.ToString();
            string aktivitaet = ((ComboBoxItem)cboAktivitaet.SelectedItem).Content.ToString();
            string ziel = ((ComboBoxItem)cboZiel.SelectedItem).Content.ToString();

            Benutzer benutzer = new Benutzer(berechnetesZiel, gewicht, groesse, alter, geschlecht, aktivitaet, ziel);
            try
            {
                benutzer.Speichere();
                MessageBox.Show("Einstellungen gespeichert");
                AppLogger.Info("Benutzereinstellungen gespeichert");
                Close();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Speichern");
                MessageBox.Show("Fehler beim Speichern.");
            }
        }

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}