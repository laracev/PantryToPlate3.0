using PantryToPlate.helpers;
using PantryToPlate.Models;
using System.Windows;
using System.Windows.Controls;

//der timer wurde mit ki gemacht (chatgpt)

namespace PantryToPlate
{
    public partial class MahlzeitHinzufuegenWindow : Window
    {
        private List<Lebensmittel> alleLebensmittel = new List<Lebensmittel>();
        private List<Lebensmittel> gefilterteLebensmittel = new List<Lebensmittel>();
        private Lebensmittel ausgewaehltesLebensmittel = null;
        private string suchtext = "";
        private bool istLaden = false;
        private System.Windows.Threading.DispatcherTimer suchTimer;

        public MahlzeitHinzufuegenWindow()
        {
            InitializeComponent();
            suchTimer = new System.Windows.Threading.DispatcherTimer();
            suchTimer.Interval = TimeSpan.FromMilliseconds(500);
            suchTimer.Tick += SuchTimer_Tick;
            LadeLebensmittel();
            ZeigeAlleLebensmittel();
            txtSuche.Text = "";
            txtGramm.Text = "100";
        }

        private void SuchTimer_Tick(object sender, EventArgs e)
        {
            suchTimer.Stop();
            FuehreSucheAus();
        }

        private void LadeLebensmittel()
        {
            try
            {
                alleLebensmittel = new List<Lebensmittel>(AppDaten.Lebensmittel);
                if (alleLebensmittel.Count == 0)
                {
                    LadeStandardLebensmittel();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Laden der Lebensmittel");
                LadeStandardLebensmittel();
            }
        }

        private void LadeStandardLebensmittel()
        {
            alleLebensmittel.Clear();
            alleLebensmittel.Add(new Lebensmittel("Apfel", 52, 0.3, 0.2, 14, 0));
            alleLebensmittel.Add(new Lebensmittel("Banane", 89, 1.1, 0.3, 23, 0));
            alleLebensmittel.Add(new Lebensmittel("Brot", 265, 9, 3.2, 49, 0));
            alleLebensmittel.Add(new Lebensmittel("Käse", 402, 25, 33, 1.3, 0));
        }

        private void ZeigeAlleLebensmittel()
        {
            gefilterteLebensmittel = new List<Lebensmittel>(alleLebensmittel);
            AktualisiereAnzeigeListe();
        }

        private void AktualisiereAnzeigeListe()
        {
            lstLebensmittel.Items.Clear();
            int maxAnzeige = gefilterteLebensmittel.Count;
            if (maxAnzeige > 100)
            {
                maxAnzeige = 100;
            }
            for (int i = 0; i < maxAnzeige; i++)
            {
                lstLebensmittel.Items.Add(gefilterteLebensmittel[i].Name);
            }
            txtAnzahlErgebnisse.Text = gefilterteLebensmittel.Count + " Lebensmittel gefunden";
        }

        private int BerechneRelevanzScore(string name)
        {
            if (string.IsNullOrWhiteSpace(suchtext))
            {
                return 0;
            }
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
            return 0;
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

        private void txtSuche_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (istLaden)
            {
                return;
            }
            suchtext = txtSuche.Text;
            suchTimer.Stop();
            suchTimer.Start();
        }

        private void btnSuchen_Click(object sender, RoutedEventArgs e)
        {
            suchTimer.Stop();
            suchtext = txtSuche.Text;
            FuehreSucheAus();
        }

        private void FuehreSucheAus()
        {
            if (istLaden)
            {
                return;
            }
            istLaden = true;
            try
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
                AktualisiereAnzeigeListe();
            }
            finally
            {
                istLaden = false;
            }
        }

        private void lstLebensmittel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (istLaden)
            {
                return;
            }
            if (lstLebensmittel.SelectedIndex >= 0 && lstLebensmittel.SelectedIndex < gefilterteLebensmittel.Count)
            {
                ausgewaehltesLebensmittel = gefilterteLebensmittel[lstLebensmittel.SelectedIndex];
                txtAusgewaehltes.Text = ausgewaehltesLebensmittel.Name;
                BerechneNaehrwerte();
            }
        }

        private void BerechneNaehrwerte()
        {
            if (ausgewaehltesLebensmittel == null)
            {
                return;
            }
            double gramm = Helper.ToDouble(txtGramm.Text);
            if (gramm <= 0)
            {
                gramm = 100;
            }
            double faktor = gramm / 100.0;
            txtKalorien.Text = (ausgewaehltesLebensmittel.KalorienPro100g * faktor).ToString("0") + " kcal";
            txtProteine.Text = (ausgewaehltesLebensmittel.ProteinePro100g * faktor).ToString("0") + " g";
            txtKohlenhydrate.Text = (ausgewaehltesLebensmittel.KohlenhydratePro100g * faktor).ToString("0") + " g";
            txtFett.Text = (ausgewaehltesLebensmittel.FettPro100g * faktor).ToString("0") + " g";
            txtBallaststoffe.Text = (ausgewaehltesLebensmittel.BallaststoffePro100g * faktor).ToString("0") + " g";
        }

        private void txtGramm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!istLaden)
            {
                BerechneNaehrwerte();
            }
        }

        private void btnHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            if (!EingabePruefung.IstGueltigeMenge(txtGramm.Text))
            {
                MessageBox.Show("Bitte eine gültige Grammzahl eingeben.");
                return;
            }
            if (ausgewaehltesLebensmittel == null)
            {
                MessageBox.Show("Bitte ein Lebensmittel auswählen.");
                return;
            }
            double gramm = Helper.ToDouble(txtGramm.Text);
            double faktor = gramm / 100.0;
            MahlzeitEintrag neuerEintrag = new MahlzeitEintrag(ausgewaehltesLebensmittel.Name, gramm, ausgewaehltesLebensmittel.KalorienPro100g * faktor, ausgewaehltesLebensmittel.ProteinePro100g * faktor, ausgewaehltesLebensmittel.KohlenhydratePro100g * faktor, ausgewaehltesLebensmittel.FettPro100g * faktor);
            try
            {
                MahlzeitEintrag.Speichere(neuerEintrag);
                MessageBox.Show(neuerEintrag.Kalorien.ToString("0") + " kcal wurden hinzugefügt");
                AppLogger.Info($"Mahlzeit hinzugefügt: {ausgewaehltesLebensmittel.Name}, {gramm}g");
                this.Close();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Speichern der Mahlzeit");
                MessageBox.Show("Fehler beim Speichern.");
            }
        }

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}