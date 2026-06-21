using PantryToPlate.helpers;
using PantryToPlate.logik;
using PantryToPlate.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PantryToPlate
{
    public partial class Window1 : Window
    {
        private double berechnetesZiel = 0;
        private KalorienzielRechner kalorienzielRechner = new KalorienzielRechner();
        private bool themeWirdGeladen;

        public double GespeichertesKalorienZiel { get; private set; }

        public Window1()
        {
            InitializeComponent();

            themeWirdGeladen = true;
            chkDarkMode.IsChecked = ThemeManager.IsDarkMode;
            themeWirdGeladen = false;

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
                    cboGeschlecht.SelectedIndex = benutzer.Geschlecht == "Weiblich" ? 0 : 1;

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
                    GespeichertesKalorienZiel = berechnetesZiel;
                    txtKalorienZiel.Text = berechnetesZiel.ToString("0");
                }
            }
            catch (IOException)
            {
                _ = MessageBox.Show("Die Benutzerdaten konnten nicht geladen werden.");
            }

            if (cboGeschlecht.SelectedIndex < 0)
            {
                cboGeschlecht.SelectedIndex = 0;
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

        //mit chatgpt
        private bool VersucheEingabenZuLesen(out double gewicht, out double groesse, out int alter)
        {
            groesse = 0;
            alter = 0;

            if (!EingabePruefung.VersucheGewichtZuLesen(txtGewicht.Text, out gewicht))
            {
                _ = MessageBox.Show("Bitte ein gültiges Gewicht zwischen 20 und 500 kg eingeben.");
                return false;
            }
            if (!EingabePruefung.VersucheGroesseZuLesen(txtGroesse.Text, out groesse))
            {
                _ = MessageBox.Show("Bitte eine gültige Größe zwischen 100 und 250 cm eingeben.");
                return false;
            }
            if (!EingabePruefung.VersucheAlterZuLesen(txtAlter.Text, out alter))
            {
                _ = MessageBox.Show("Bitte ein gültiges Alter zwischen 14 und 120 Jahren eingeben.");
                return false;
            }
            if (cboGeschlecht.SelectedItem != null && cboAktivitaet.SelectedItem != null && cboZiel.SelectedItem != null)
            {
                return true;
            }

            _ = MessageBox.Show("Bitte alle Felder auswählen.");
            return false;
        }
        //chatgpt ende

        private double ErmittleAktivitaetsfaktor()
        {
            double[] faktoren = { 1.2, 1.375, 1.55, 1.725 };

            if (cboAktivitaet.SelectedIndex >= 0 &&
                cboAktivitaet.SelectedIndex < faktoren.Length)
            {
                return faktoren[cboAktivitaet.SelectedIndex];
            }

            return 1.2;
        }

        private string ErmittleZiel()
        {
            if (cboZiel.SelectedIndex == 0)
            {
                return "Abnehmen";
            }

            if (cboZiel.SelectedIndex == 2)
            {
                return "Zunehmen";
            }

            return "Halten";
        }

        private void BerechneZiel(double gewicht, double groesse, int alter)
        {
            string geschlecht = ((ComboBoxItem)cboGeschlecht.SelectedItem).Content.ToString();
            double aktivitaetsfaktor = ErmittleAktivitaetsfaktor();
            string ziel = ErmittleZiel();

            berechnetesZiel = kalorienzielRechner.BerechneKalorienziel(gewicht, groesse, alter, geschlecht, aktivitaetsfaktor, ziel);

            txtKalorienZiel.Text = berechnetesZiel.ToString("0");
        }

        private void btnBerechnen_Click(object sender, RoutedEventArgs e)
        {
            if (!VersucheEingabenZuLesen(out double gewicht, out double groesse, out int alter))
            {
                return;
            }

            BerechneZiel(gewicht, groesse, alter);
        }
        //teilweise mit chatgpt

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            if (!VersucheEingabenZuLesen(out double gewicht, out double groesse, out int alter))
            {
                return;
            }

            BerechneZiel(gewicht, groesse, alter);

            try
            {
                string geschlecht = ((ComboBoxItem)cboGeschlecht.SelectedItem).Content.ToString();
                string aktivitaet = ((ComboBoxItem)cboAktivitaet.SelectedItem).Content.ToString();
                string ziel = ((ComboBoxItem)cboZiel.SelectedItem).Content.ToString();

                Benutzer benutzer = new Benutzer(berechnetesZiel, gewicht, groesse, alter, geschlecht, aktivitaet, ziel);
                try
                {
                    benutzer.Speichere();
                    GespeichertesKalorienZiel = berechnetesZiel;
                    _ = MessageBox.Show("Einstellungen gespeichert");
                    AppLogger.Info("Benutzereinstellungen gespeichert");
                    DialogResult = true;
                }
                catch (IOException)
                {
                    _ = MessageBox.Show("Die Einstellungen konnten nicht gespeichert werden.");
                }

            }
            catch
            {
                AppLogger.Error("Es gab schwierigkeiten beim speichern. haben sie vielleicht nicht das bilogische geschlecht ausgewählt?");
            }


        }
        //chatgpt ende

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void chkDarkMode_Changed(object sender, RoutedEventArgs e)
        {
            if (themeWirdGeladen)
            {
                return;
            }

            ThemeManager.SetDarkMode(chkDarkMode.IsChecked == true);
        }
    }
}
