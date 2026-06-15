using PantryToPlate.helpers;
using PantryToPlate.logik;
using PantryToPlate.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PantryToPlate
{
    public partial class FitnessWindow : Window
    {
        private List<MetEintrag> metEintraege = new List<MetEintrag>();
        private double benutzerGewicht = 70;
        private FitnessRechner fitnessRechner = new FitnessRechner();

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
            try
            {
                metEintraege = MetEintrag.LadeAlleAusCsv();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Laden der MET-Werte");
                metEintraege = new List<MetEintrag>();
            }
        }

        private void LadeBenutzerGewicht()
        {
            try
            {
                Benutzer benutzer = Benutzer.LadeAusCsv();
                if (benutzer != null && benutzer.Gewicht > 0)
                {
                    benutzerGewicht = benutzer.Gewicht;
                }
                else
                {
                    benutzerGewicht = 70;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Laden des Benutzergewichts");
                benutzerGewicht = 70;
            }
        }

        private void BefuelleComboBox()
        {
            cboAktivitaet.Items.Clear();
            foreach (MetEintrag met in metEintraege)
            {
                cboAktivitaet.Items.Add(met.Aktivitaet + " (MET " + met.MetWert.ToString("0.0") + ")");
            }
            if (cboAktivitaet.Items.Count > 0)
            {
                cboAktivitaet.SelectedIndex = 0;
            }
        }

        private void BerechneKalorien()
        {
            double dauerMinuten = Namensvergleich.ZahlLesen(txtDauer.Text);
            if (dauerMinuten <= 0)
            {
                txtVerbrannteKalorien.Text = "0 kcal";
                return;
            }
            double metWert = (cboAktivitaet.SelectedIndex >= 0 && cboAktivitaet.SelectedIndex < metEintraege.Count) ? metEintraege[cboAktivitaet.SelectedIndex].MetWert : 1;
            double kalorien = fitnessRechner.BerechneKalorien(metWert, benutzerGewicht, dauerMinuten);
            txtVerbrannteKalorien.Text = kalorien.ToString("0") + " kcal";
        }

        private void LadeHeutigeAktivitaeten()
        {
            lstAktivitaeten.Items.Clear();
            try
            {
                List<FitnessEintrag> eintraege = FitnessEintrag.LadeVonTag(DateTime.Now);
                foreach (FitnessEintrag f in eintraege)
                {
                    lstAktivitaeten.Items.Add(f.Aktivitaet + " - " + f.VerbrannteKalorien.ToString("0") + " kcal");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Laden der heutigen Fitnessaktivitäten");
            }
        }

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            if (!EingabePruefung.IstGueltigeDauer(txtDauer.Text))
            {
                MessageBox.Show("Bitte eine gültige Dauer (1-1440 Minuten) eingeben.");
                return;
            }
            double dauerMinuten = Namensvergleich.ZahlLesen(txtDauer.Text);
            if (dauerMinuten <= 0)
            {
                MessageBox.Show("Dauer muss größer als 0 sein.");
                return;
            }

            string aktivitaetText = metEintraege[cboAktivitaet.SelectedIndex].Aktivitaet;
            double metWert = metEintraege[cboAktivitaet.SelectedIndex].MetWert;
            double kalorien = fitnessRechner.BerechneKalorien(metWert, benutzerGewicht, dauerMinuten);
            FitnessEintrag neuerEintrag = new FitnessEintrag(DateTime.Now, aktivitaetText, kalorien);
            try
            {
                FitnessEintrag.Speichere(neuerEintrag);
                MessageBox.Show(kalorien.ToString("0") + " kcal wurden gespeichert!\n" + "Das entspricht " + dauerMinuten + " Minuten " + aktivitaetText);
                AppLogger.Info($"Fitness gespeichert: {aktivitaetText}, {dauerMinuten} min, {kalorien} kcal");
                LadeHeutigeAktivitaeten();
                txtDauer.Text = "30";
                cboAktivitaet.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Speichern des Fitnesseintrags");
                MessageBox.Show("Fehler beim Speichern.");
            }
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