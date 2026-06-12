using PantryToPlate.Models;
using PTP;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PantryToPlate
{
    public partial class MahlzeitHinzufuegenWindow : Window
    {
        private List<Lebensmittel> alleLebensmittel = new List<Lebensmittel>();
        private List<Lebensmittel> gefilterteLebensmittel = new List<Lebensmittel>();
        private Lebensmittel ausgewaehltesLebensmittel = null;
        private string suchtext = "";
        private bool istLaden = false;
        private System.Windows.Threading.DispatcherTimer suchTimer; //KI

        public MahlzeitHinzufuegenWindow()
        {
            InitializeComponent();

            //ki start: promt: mach ma dispatcher timer der 500ms nach tippstopp die suche ausführt, damit nicht bei jedem buchstaben die suche ausgeführt wird
            suchTimer = new System.Windows.Threading.DispatcherTimer();
            suchTimer.Interval = TimeSpan.FromMilliseconds(500);
            suchTimer.Tick += SuchTimer_Tick;
            //ki ende
            alleLebensmittel.Clear();

            for (int i = 0; i < AppDaten.Lebensmittel.Count; i++)
            {
                alleLebensmittel.Add(AppDaten.Lebensmittel[i]);
            }

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
            istLaden = true;

            string datei = "data/test_utf8.csv";
            alleLebensmittel.Clear();

            if (File.Exists(datei))
            {
                string[] zeilen = File.ReadAllLines(datei);
                int maxEintraege = Math.Min(zeilen.Length, 5000);

                for (int i = 1; i < maxEintraege; i++)
                {
                    string zeile = zeilen[i];
                    if (string.IsNullOrWhiteSpace(zeile)) continue;

                    string[] teile = zeile.Split(';');
                    if (teile.Length >= 5)
                    {
                        try
                        {
                            Lebensmittel lm = new Lebensmittel();
                            lm.Name = teile[0].Trim();

                            if (string.IsNullOrWhiteSpace(lm.Name)) continue;

                            double kalorien = Helper.ToDouble(teile[1]);
                            double proteine = Helper.ToDouble(teile[2]);
                            double fett = Helper.ToDouble(teile[3]);
                            double kohlen = Helper.ToDouble(teile[4]);

                            lm.KalorienPro100g = kalorien;
                            lm.ProteinePro100g = proteine;
                            lm.FettPro100g = fett;
                            lm.KohlenhydratePro100g = kohlen;

                            if (teile.Length >= 6)
                            {
                                lm.BallaststoffePro100g = Helper.ToDouble(teile[5]);
                            }

                            alleLebensmittel.Add(lm);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }

            if (alleLebensmittel.Count == 0)
            {
                MessageBox.Show("Keine Lebensmittel-Datei gefunden. Verwende Standarddaten.");
                LadeStandardLebensmittel();
            }

            ZeigeAlleLebensmittel();
            istLaden = false;
        }

        private void LadeStandardLebensmittel()
        {
            Lebensmittel apfel = new Lebensmittel();
            apfel.Name = "Apfel";
            apfel.KalorienPro100g = 52;
            apfel.ProteinePro100g = 0.3;
            apfel.KohlenhydratePro100g = 14;
            apfel.FettPro100g = 0.2;
            alleLebensmittel.Add(apfel);

            Lebensmittel banane = new Lebensmittel();
            banane.Name = "Banane";
            banane.KalorienPro100g = 89;
            banane.ProteinePro100g = 1.1;
            banane.KohlenhydratePro100g = 23;
            banane.FettPro100g = 0.3;
            alleLebensmittel.Add(banane);

            Lebensmittel brot = new Lebensmittel();
            brot.Name = "Brot";
            brot.KalorienPro100g = 265;
            brot.ProteinePro100g = 9;
            brot.KohlenhydratePro100g = 49;
            brot.FettPro100g = 3.2;
            alleLebensmittel.Add(brot);

            Lebensmittel kaese = new Lebensmittel();
            kaese.Name = "Käse";
            kaese.KalorienPro100g = 402;
            kaese.ProteinePro100g = 25;
            kaese.KohlenhydratePro100g = 1.3;
            kaese.FettPro100g = 33;
            alleLebensmittel.Add(kaese);
        }

        private void ZeigeAlleLebensmittel()
        {
            gefilterteLebensmittel = new List<Lebensmittel>();
            for (int i = 0; i < alleLebensmittel.Count; i++)
            {
                gefilterteLebensmittel.Add(alleLebensmittel[i]);
            }
            AktualisiereAnzeigeListe();
        }

        // Diese Methode ersetzt die Lambda-Funktion!
        private void AktualisiereAnzeigeImUIThread()
        {
            lstLebensmittel.Items.Clear();

            int maxAnzeige = Math.Min(gefilterteLebensmittel.Count, 100);
            for (int i = 0; i < maxAnzeige; i++)
            {
                string anzeige = gefilterteLebensmittel[i].Name;
                lstLebensmittel.Items.Add(anzeige);
            }

            txtAnzahlErgebnisse.Text = gefilterteLebensmittel.Count + " Lebensmittel gefunden";
        }

        private void AktualisiereAnzeigeListe()
        {
            AktualisiereAnzeigeImUIThread();
        }

        // im sorry aber das muss ic´h mit lamdba machen, sonst crashed alles weil zu viele sachen
        private void SortiereNachRelevanz()
        {
            gefilterteLebensmittel.Sort((a, b) =>
                BerechneRelevanzScore(b.Name).CompareTo(BerechneRelevanzScore(a.Name)));
        }

       

        private int BerechneRelevanzScore(string name)
        {
            if (string.IsNullOrWhiteSpace(suchtext)) return 0;

            string nameLower = name.ToLower();
            string suchLower = suchtext.ToLower();

            if (nameLower == suchLower) return 1000;
            if (nameLower.StartsWith(suchLower)) return 500;
            if (nameLower.Contains(suchLower)) return 100;

            return 0;
        }

        private void txtSuche_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (istLaden) return;

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
            if (istLaden) return;

            istLaden = true;


            try
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
                        string name = alleLebensmittel[i].Name.ToLower();
                        if (name.Contains(suchLower))
                        {
                            gefilterteLebensmittel.Add(alleLebensmittel[i]);
                        }
                    }
                }

                SortiereNachRelevanz();
                AktualisiereAnzeigeListe();
            }
            //ki start: promt: broski hilf mir alles kaputt
            finally
            {
                istLaden = false;
            }
            //KI ende
        }

        private void lstLebensmittel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (istLaden) return;

            if (lstLebensmittel.SelectedIndex >= 0 && lstLebensmittel.SelectedIndex < gefilterteLebensmittel.Count)
            {
                ausgewaehltesLebensmittel = gefilterteLebensmittel[lstLebensmittel.SelectedIndex];
                txtAusgewaehltes.Text = ausgewaehltesLebensmittel.Name;
                BerechneNaehrwerte();
            }
        }

        private void BerechneNaehrwerte()
        {
            if (ausgewaehltesLebensmittel == null) return;

            double gramm = Helper.ToDouble(txtGramm.Text);
            if (gramm <= 0) gramm = 100;

            double faktor = gramm / 100.0;

            double kalorien = ausgewaehltesLebensmittel.KalorienPro100g * faktor;
            double proteine = ausgewaehltesLebensmittel.ProteinePro100g * faktor;
            double kohlen = ausgewaehltesLebensmittel.KohlenhydratePro100g * faktor;
            double fett = ausgewaehltesLebensmittel.FettPro100g * faktor;
            double ballast = ausgewaehltesLebensmittel.BallaststoffePro100g * faktor;

            txtKalorien.Text = "Kalorien: " + kalorien.ToString("0") + " kcal";
            txtProteine.Text = "Proteine: " + proteine.ToString("0") + " g";
            txtKohlenhydrate.Text = "Kohlenhydrate: " + kohlen.ToString("0") + " g";
            txtFett.Text = "Fett: " + fett.ToString("0") + " g";
            txtBallaststoffe.Text = "Ballaststoffe: " + ballast.ToString("0") + " g";
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
            if (ausgewaehltesLebensmittel == null)
            {
                MessageBox.Show("Bitte ein Lebensmittel auswählen");
                return;
            }

            double gramm = Helper.ToDouble(txtGramm.Text);
            if (gramm <= 0)
            {
                MessageBox.Show("Bitte gültige Grammzahl eingeben");
                return;
            }

            double faktor = gramm / 100.0;
            double kalorien = ausgewaehltesLebensmittel.KalorienPro100g * faktor;
            double proteine = ausgewaehltesLebensmittel.ProteinePro100g * faktor;
            double kohlen = ausgewaehltesLebensmittel.KohlenhydratePro100g * faktor;
            double fett = ausgewaehltesLebensmittel.FettPro100g * faktor;

            string heute = DateTime.Now.ToString("yyyy-MM-dd");
            string zeile = heute + ";" + ausgewaehltesLebensmittel.Name + ";" + gramm + ";" + kalorien + ";" + proteine + ";" + kohlen + ";" + fett;

            Directory.CreateDirectory("data");
            string datei = "data/tagesmahlzeiten.csv";

            if (!File.Exists(datei))
            {
                File.WriteAllText(datei, "Datum;Lebensmittel;Gramm;Kalorien;Proteine;Kohlenhydrate;Fett\n" + zeile);
            }
            else
            {
                File.AppendAllText(datei, "\n" + zeile);
            }

            MessageBox.Show(kalorien.ToString("0") + " kcal wurden hinzugefügt");
            this.Close();
        }

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}