using PantryToPlate.Models;
using PTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace PantryToPlate
{
    public partial class FitnessWindow : Window
    {
        private string fitnessDatei = "data/FitnessEintraege.csv";
        private string metDatei = "data/MET_Werte_Tabelle.csv";
        private List<MetEintrag> metEintraege = new List<MetEintrag>();
        private double benutzerGewicht = 70;
        private string benutzerDatei = "data/benutzer.csv";

        public FitnessWindow()
        {
            InitializeComponent();

            LadeMetEintraege();
            LadeBenutzerGewicht();
            LadeHeutigeAktivitaeten();
            BefuelleComboBox();
            txtDauer.Text = "30";
            BerechneKalorien();
        }

        private void LadeMetEintraege()
        {
            metEintraege.Clear();

            if (File.Exists(metDatei))
            {
                string[] zeilen = File.ReadAllLines(metDatei);
                for (int i = 1; i < zeilen.Length; i++)
                {
                    string[] teile = zeilen[i].Split(';');
                    if (teile.Length >= 2)
                    {
                        MetEintrag met = new MetEintrag();
                        met.Aktivitaet = teile[0].Trim();

                        string metText = teile[1].Trim();
                        double metWert = ZahlLesen(metText);
                        met.MetWert = metWert;

                        metEintraege.Add(met);
                    }
                }
            }
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

        private void BefuelleComboBox()
        {
            cboAktivitaet.Items.Clear();
            for (int i = 0; i < metEintraege.Count; i++)
            {
                cboAktivitaet.Items.Add(metEintraege[i].Aktivitaet + " (MET " + metEintraege[i].MetWert.ToString("0.0") + ")");
            }

            if (cboAktivitaet.Items.Count > 0)
            {
                cboAktivitaet.SelectedIndex = 0;
            }
        }

        private void LadeBenutzerGewicht()
        {
            if (File.Exists(benutzerDatei))
            {
                string[] zeilen = File.ReadAllLines(benutzerDatei);
                if (zeilen.Length > 1)
                {
                    string[] teile = zeilen[1].Split(';');
                    if (teile.Length >= 2)
                    {
                        benutzerGewicht = ZahlLesen(teile[1]);
                    }
                }
            }

            if (benutzerGewicht <= 0) benutzerGewicht = 70;
        }

        private void BerechneKalorien()
        {
            double dauerMinuten = ZahlLesen(txtDauer.Text);
            if (dauerMinuten <= 0)
            {
                txtVerbrannteKalorien.Text = "0 kcal";
                return;
            }

            double metWert = HoleMETWert();
            double dauerStunden = dauerMinuten / 60.0;

            double kalorien = metWert * benutzerGewicht * dauerStunden;

            txtVerbrannteKalorien.Text = kalorien.ToString("0") + " kcal";
        }

        private double HoleMETWert()
        {
            if (cboAktivitaet.SelectedIndex >= 0 && cboAktivitaet.SelectedIndex < metEintraege.Count)
            {
                return metEintraege[cboAktivitaet.SelectedIndex].MetWert;
            }
            return 1;
        }

        private void LadeHeutigeAktivitaeten()
        {
            lstAktivitaeten.Items.Clear();
            string heute = DateTime.Now.ToString("yyyy-MM-dd");

            if (File.Exists(fitnessDatei))
            {
                string[] zeilen = File.ReadAllLines(fitnessDatei);
                for (int i = 1; i < zeilen.Length; i++)
                {
                    string[] teile = zeilen[i].Split(';');
                    if (teile.Length >= 3 && teile[0] == heute)
                    {
                        lstAktivitaeten.Items.Add(teile[1] + " - " + teile[2] + " kcal");
                    }
                }
            }
        }

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            if (cboAktivitaet.SelectedIndex < 0)
            {
                MessageBox.Show("Bitte eine Aktivität auswählen");
                return;
            }

            double dauerMinuten = ZahlLesen(txtDauer.Text);
            if (dauerMinuten <= 0)
            {
                MessageBox.Show("Bitte eine gültige Dauer eingeben");
                return;
            }

            string aktivitaetText = metEintraege[cboAktivitaet.SelectedIndex].Aktivitaet;
            double metWert = metEintraege[cboAktivitaet.SelectedIndex].MetWert;
            double dauerStunden = dauerMinuten / 60.0;
            double kalorien = metWert * benutzerGewicht * dauerStunden;

            string heute = DateTime.Now.ToString("yyyy-MM-dd");
            string zeile = heute + ";" + aktivitaetText + ";" + kalorien.ToString("0");

            Directory.CreateDirectory("data");

            if (File.Exists(fitnessDatei))
            {
                File.AppendAllText(fitnessDatei, "\n" + zeile);
            }
            else
            {
                File.WriteAllText(fitnessDatei, "Datum;Aktivitaet;Kalorien\n" + zeile);
            }

            MessageBox.Show(kalorien.ToString("0") + " kcal wurden gespeichert!\n" +
                           "Das entspricht " + dauerMinuten + " Minuten " + aktivitaetText);

            LadeHeutigeAktivitaeten();
            txtDauer.Text = "30";
            cboAktivitaet.SelectedIndex = 0;
        }

        private void cboAktivitaet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BerechneKalorien();
        }

        private void txtDauer_TextChanged(object sender, TextChangedEventArgs e)
        {
            BerechneKalorien();
        }

        private void btnSchliessen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}