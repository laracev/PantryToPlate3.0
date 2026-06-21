using PantryToPlate.helpers;
using PantryToPlate.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PantryToPlate
{
    public partial class PantryWindow : Window
    {
        private List<Lebensmittel> alleLebensmittel = new List<Lebensmittel>();
        private List<Lebensmittel> gefilterteLebensmittel = new List<Lebensmittel>();
        private List<PantryItem> pantryItems = new List<PantryItem>();
        private Lebensmittel ausgewaehltesLebensmittel = null;
        private string suchtext = "";
        private System.Windows.Threading.DispatcherTimer suchTimer;

        public PantryWindow()
        {
            InitializeComponent();
            this.Activated += PantryWindow_Activated;
            suchTimer = new System.Windows.Threading.DispatcherTimer();
            suchTimer.Interval = TimeSpan.FromMilliseconds(400);
            suchTimer.Tick += SuchTimer_Tick;
            LadeLebensmittel();
            LadePantry();
            ZeigePantry();
            txtMenge.Text = "100";
        }

        private void PantryWindow_Activated(object sender, EventArgs e)
        {
            LadePantry();
            ZeigePantry();
        }

        private void LadeLebensmittel()
        {
            try
            {
                alleLebensmittel = new List<Lebensmittel>(AppDaten.Lebensmittel);
                ZeigeAlleLebensmittel();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Laden der Lebensmittel");
                alleLebensmittel = new List<Lebensmittel>();
            }
        }

        private void LadePantry()
        {
            try
            {
                pantryItems = AppDaten.Pantry;
            }
            catch
            {
                AppLogger.Error("Fehler beim Laden der Pantry");
                pantryItems = new List<PantryItem>();
            }
        }

        private void SpeicherePantry()
        {
            try
            {
                PantryItem.SpeichereAlle(pantryItems);
            }
            catch
            {
                AppLogger.Error("Fehler beim Speichern der Pantry");
                MessageBox.Show("Die Pantry konnte nicht gespeichert werden.");
            }
        }

        private void ZeigePantry()
        {
            dgPantry.ItemsSource = null;
            dgPantry.ItemsSource = pantryItems;
        }

        private void ZeigeAlleLebensmittel()
        {
            gefilterteLebensmittel = new List<Lebensmittel>(alleLebensmittel);
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

        private int BerechneRelevanzScore(string name)
        {
            string nameLower = name.ToLower();
            string suchLower = suchtext.ToLower();
            if (nameLower == suchLower)
            {
                return 1000;
            }
            if (nameLower.StartsWith(suchLower))
            {
                return 500;
            }
            if (nameLower.Contains(suchLower))
            {
                return 100;
            }
            int matches = 0;
            int maxLength = suchLower.Length;
            if (maxLength > nameLower.Length)
            {
                maxLength = nameLower.Length;
            }
            for (int i = 0; i < maxLength; i++)
            {
                if (nameLower[i] == suchLower[i])
                {
                    matches++;
                }
            }
            return matches;
        }

        private void SortiereNachRelevanz()
        {
            for (int i = 0; i < gefilterteLebensmittel.Count - 1; i++)
            {
                for (int j = i + 1; j < gefilterteLebensmittel.Count; j++)
                {
                    int scoreA = BerechneRelevanzScore(gefilterteLebensmittel[i].Name);
                    int scoreB = BerechneRelevanzScore(gefilterteLebensmittel[j].Name);
                    if (scoreB > scoreA)
                    {
                        Lebensmittel temp = gefilterteLebensmittel[i];
                        gefilterteLebensmittel[i] = gefilterteLebensmittel[j];
                        gefilterteLebensmittel[j] = temp;
                    }
                }
            }
        }

        private void SuchTimer_Tick(object sender, EventArgs e)
        {
            suchTimer.Stop();
            FuehreSucheAus();
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
                gefilterteLebensmittel.AddRange(alleLebensmittel);
            }
            else
            {
                string suchLower = suchtext.ToLower();
                foreach (Lebensmittel lm in alleLebensmittel)
                {
                    if (lm.Name.ToLower().Contains(suchLower))
                    {
                        gefilterteLebensmittel.Add(lm);
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
            if (!EingabePruefung.IstGueltigeMenge(txtMenge.Text))
            {
                MessageBox.Show("Bitte eine gültige Menge eingeben.");
                return;
            }
            if (ausgewaehltesLebensmittel == null)
            {
                MessageBox.Show("Bitte ein Lebensmittel auswählen.");
                return;
            }
            double menge = Helper.ToDouble(txtMenge.Text);
            bool gefunden = false;
            foreach (PantryItem item in pantryItems)
            {
                if (item.Name == ausgewaehltesLebensmittel.Name)
                {
                    item.ErhoeheMenge(menge);
                    gefunden = true;
                    break;
                }
            }
            if (!gefunden)
            {
                pantryItems.Add(new PantryItem(ausgewaehltesLebensmittel.Name, menge));
            }
            SpeicherePantry();
            ZeigePantry();
            MessageBox.Show(ausgewaehltesLebensmittel.Name + " (" + menge + "g) wurde zur Pantry hinzugefügt");
            AppLogger.Info($"Pantry hinzugefügt: {ausgewaehltesLebensmittel.Name}, {menge}g");
            txtMenge.Text = "100";
        }

        private void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            PantryItem item = (PantryItem)btn.Tag;
            if (item == null)
            {
                return;
            }
            if (MessageBox.Show(item.Name + " wirklich aus der Pantry löschen?", "Bestätigen", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                pantryItems.Remove(item);
                SpeicherePantry();
                ZeigePantry();
                AppLogger.Info($"Aus Pantry gelöscht: {item.Name}");
            }
        }
        private void btnSchliessen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}