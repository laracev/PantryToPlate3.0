using PantryToPlate.Models;
using PTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PantryToPlate
{
    public partial class PantryWindow : Window
    {
        private List<Lebensmittel> alleLebensmittel = new List<Lebensmittel>();
        private List<Lebensmittel> gefilterteLebensmittel = new List<Lebensmittel>();
        private List<PantryItem> pantryItems = new List<PantryItem>();
        private Lebensmittel ausgewaehltesLebensmittel = null;
        private string suchtext = "";
        private string pantryDatei = "data/pantry.csv";

        private List<string> lebensmittelNamen = new List<string>();
        private System.Windows.Threading.DispatcherTimer suchTimer;

        public PantryWindow()
        {
            InitializeComponent();

            suchTimer = new System.Windows.Threading.DispatcherTimer();
            suchTimer.Interval = TimeSpan.FromMilliseconds(400);
            suchTimer.Tick += SuchTimer_Tick;

            LadeLebensmittel();
            LadePantry();
            ZeigePantry();
            txtMenge.Text = "100";
        }


        private double ZahlLesen(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            text = text.Trim();
            text = text.Replace(" kcal", "");
            text = text.Replace("g", "");
            text = text.Replace(" ", "");

            if (text.Contains(",") && !text.Contains("."))
            {
                text = text.Replace(",", ".");
            }

            try
            {
                return Convert.ToDouble(text, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        private void SuchTimer_Tick(object sender, EventArgs e)
        {
            suchTimer.Stop();
            FuehreSucheAus();
        }

        private void LadeLebensmittel()
        {
            string datei = "data/test_utf8.csv";
            alleLebensmittel.Clear();
            lebensmittelNamen.Clear();

            if (File.Exists(datei))
            {
                string[] zeilen = File.ReadAllLines(datei);

                for (int i = 1; i < zeilen.Length; i++)
                {
                    string zeile = zeilen[i];
                    if (string.IsNullOrWhiteSpace(zeile)) continue;

                    string[] teile = zeile.Split(';');
                    if (teile.Length >= 5)
                    {
                        Lebensmittel lm = new Lebensmittel();
                        lm.Name = teile[0].Trim();
                        lebensmittelNamen.Add(lm.Name);

                        double kalorien = ZahlLesen(teile[1]);
                        double proteine = ZahlLesen(teile[2]);
                        double fett = ZahlLesen(teile[3]);
                        double kohlen = ZahlLesen(teile[4]);

                        lm.KalorienPro100g = kalorien;
                        lm.ProteinePro100g = proteine;
                        lm.FettPro100g = fett;
                        lm.KohlenhydratePro100g = kohlen;

                        lm.BallaststoffePro100g = 0;
                        if (teile.Length >= 6)
                        {
                            lm.BallaststoffePro100g = ZahlLesen(teile[5]);
                        }

                        alleLebensmittel.Add(lm);
                    }
                }
            }

            ZeigeAlleLebensmittel();
        }

        private void LadePantry()
        {
            pantryItems.Clear();

            if (File.Exists(pantryDatei))
            {
                string[] zeilen = File.ReadAllLines(pantryDatei);
                for (int i = 1; i < zeilen.Length; i++)
                {
                    string[] teile = zeilen[i].Split(';');
                    if (teile.Length >= 2)
                    {
                        PantryItem item = new PantryItem();
                        item.Name = teile[0];
                        double menge = ZahlLesen(teile[1]);
                        item.Menge = menge;
                        pantryItems.Add(item);
                    }
                }
            }
        }

        private void SpeicherePantry()
        {
            string inhalt = "Name;Menge\n";
            for (int i = 0; i < pantryItems.Count; i++)
            {
                if (pantryItems[i].Menge > 0)
                {
                    inhalt = inhalt + pantryItems[i].Name + ";" + pantryItems[i].Menge + "\n";
                }
            }

            Directory.CreateDirectory("data");
            File.WriteAllText(pantryDatei, inhalt);
        }

        private void ZeigePantry()
        {
            dgPantry.ItemsSource = null;
            dgPantry.ItemsSource = pantryItems;
        }

        private void ZeigeAlleLebensmittel()
        {
            gefilterteLebensmittel = new List<Lebensmittel>();
            for (int i = 0; i < alleLebensmittel.Count; i++)
            {
                gefilterteLebensmittel.Add(alleLebensmittel[i]);
            }
            AktualisiereLebensmittelListe();
        }

        private void AktualisiereLebensmittelListe()
        {
            lstLebensmittel.Items.Clear();

            int maxAnzeige = gefilterteLebensmittel.Count;
            if (maxAnzeige > 50)
            {
                maxAnzeige = 50;
            }

            for (int i = 0; i < maxAnzeige; i++)
            {
                lstLebensmittel.Items.Add(gefilterteLebensmittel[i].Name);
            }

            txtAnzahlErgebnisse.Text = gefilterteLebensmittel.Count + " Lebensmittel gefunden";
        }

        private void SortiereNachRelevanz()
        {
            for (int i = 0; i < gefilterteLebensmittel.Count - 1; i++)
            {
                for (int j = i + 1; j < gefilterteLebensmittel.Count; j++)
                {
                    int scoreI = BerechneRelevanzScore(gefilterteLebensmittel[i].Name);
                    int scoreJ = BerechneRelevanzScore(gefilterteLebensmittel[j].Name);

                    if (scoreI < scoreJ)
                    {
                        Lebensmittel temp = gefilterteLebensmittel[i];
                        gefilterteLebensmittel[i] = gefilterteLebensmittel[j];
                        gefilterteLebensmittel[j] = temp;
                    }
                }
            }
        }

        private int BerechneRelevanzScore(string name)
        {
            string nameLower = name.ToLower();
            string suchLower = suchtext.ToLower();

            if (nameLower == suchLower) return 1000;
            if (nameLower.StartsWith(suchLower)) return 500;
            if (nameLower.Contains(suchLower)) return 100;

            int matches = 0;
            int maxLength = suchLower.Length;
            if (maxLength > nameLower.Length)
            {
                maxLength = nameLower.Length;
            }

            for (int i = 0; i < maxLength; i++)
            {
                if (nameLower[i] == suchLower[i]) matches++;
            }
            return matches;
        }

        private void txtSuche_TextChanged(object sender, TextChangedEventArgs e)
        {
            suchtext = txtSuche.Text;
            suchTimer.Stop();
            suchTimer.Start();
        }

        private void FuehreSucheAus()
        {
            gefilterteLebensmittel.Clear();

            if (string.IsNullOrWhiteSpace(suchtext))
            {
                for (int i = 0; i < alleLebensmittel.Count; i++)
                {
                    gefilterteLebensmittel.Add(alleLebensmittel[i]);
                }
            }
            else
            {
                string suchLower = suchtext.ToLower();
                for (int i = 0; i < alleLebensmittel.Count; i++)
                {
                    if (alleLebensmittel[i].Name.ToLower().Contains(suchLower))
                    {
                        gefilterteLebensmittel.Add(alleLebensmittel[i]);
                    }
                }
            }

            SortiereNachRelevanz();
            AktualisiereLebensmittelListe();
        }

        private void btnAlleAnzeigen_Click(object sender, RoutedEventArgs e)
        {
            txtSuche.Text = "";
            suchtext = "";
            FuehreSucheAus();
        }

        private void lstLebensmittel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLebensmittel.SelectedIndex >= 0 && lstLebensmittel.SelectedIndex < gefilterteLebensmittel.Count)
            {
                ausgewaehltesLebensmittel = gefilterteLebensmittel[lstLebensmittel.SelectedIndex];
                txtAusgewaehltes.Text = ausgewaehltesLebensmittel.Name;
            }
        }

        private void btnZurPantryHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            if (ausgewaehltesLebensmittel == null)
            {
                MessageBox.Show("Bitte ein Lebensmittel auswählen");
                return;
            }

            double menge = ZahlLesen(txtMenge.Text);
            if (menge <= 0)
            {
                MessageBox.Show("Bitte eine gültige Menge eingeben");
                return;
            }

            bool gefunden = false;
            for (int i = 0; i < pantryItems.Count; i++)
            {
                if (pantryItems[i].Name.ToLower() == ausgewaehltesLebensmittel.Name.ToLower())
                {
                    pantryItems[i].Menge = pantryItems[i].Menge + menge;
                    gefunden = true;
                    break;
                }
            }

            if (!gefunden)
            {
                PantryItem neuesItem = new PantryItem();
                neuesItem.Name = ausgewaehltesLebensmittel.Name;
                neuesItem.Menge = menge;
                pantryItems.Add(neuesItem);
            }

            SpeicherePantry();
            ZeigePantry();

            MessageBox.Show(ausgewaehltesLebensmittel.Name + " (" + menge + "g) wurde zur Pantry hinzugefügt");

            txtMenge.Text = "100";
        }

        private void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            PantryItem item = (PantryItem)btn.Tag;

            if (item != null)
            {
                MessageBoxResult result = MessageBox.Show(item.Name + " wirklich aus der Pantry löschen?",
                    "Bestätigen", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    pantryItems.Remove(item);
                    SpeicherePantry();
                    ZeigePantry();
                }
            }
        }

        private void btnSchliessen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}