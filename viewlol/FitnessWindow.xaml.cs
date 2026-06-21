using PantryToPlate.helpers;
using PantryToPlate.logik;
using PantryToPlate.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PantryToPlate
{
    public partial class FitnessWindow : Window
    {
        private List<MetEintrag> metEintraege = new List<MetEintrag>();
        private List<FitnessEintrag> heutigeAktivitaeten = new List<FitnessEintrag>();
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
            metEintraege = new List<MetEintrag>(AppDaten.MetEintraege);
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
            catch (IOException)
            {
                AppLogger.Error("Fehler beim Laden des Benutzergewichts");
                benutzerGewicht = 70;
            }
        }

        private void BefuelleComboBox()
        {
            cboAktivitaet.Items.Clear();

            foreach (MetEintrag metEintrag in metEintraege)
            {
                cboAktivitaet.Items.Add(metEintrag.Aktivitaet + " (MET " + metEintrag.MetWert.ToString("0.0") + ")");
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

            double metWert = 1;

            if (cboAktivitaet.SelectedIndex >= 0 && cboAktivitaet.SelectedIndex < metEintraege.Count)
            {
                metWert = metEintraege[cboAktivitaet.SelectedIndex].MetWert;
            }

            double kalorien = fitnessRechner.BerechneKalorien(metWert, benutzerGewicht, dauerMinuten);

            txtVerbrannteKalorien.Text = kalorien.ToString("0") + " kcal";
        }

        private void LadeHeutigeAktivitaeten()
        {
            lstAktivitaeten.Items.Clear();

            try
            {
                heutigeAktivitaeten = FitnessEintrag.LadeVonTag(DateTime.Now);

                foreach (FitnessEintrag eintrag in heutigeAktivitaeten)
                {
                    lstAktivitaeten.Items.Add(eintrag.Aktivitaet + " - " + eintrag.VerbrannteKalorien.ToString("0") + " kcal");
                }
            }
            catch
            {
                AppLogger.Error("Fehler beim Laden der heutigen Fitnessaktivitäten");
                heutigeAktivitaeten = new List<FitnessEintrag>();
            }
        }

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            if (cboAktivitaet.SelectedIndex < 0 || cboAktivitaet.SelectedIndex >= metEintraege.Count)
            {
                MessageBox.Show("Bitte eine gültige Aktivität auswählen.");
                return;
            }

            if (!EingabePruefung.IstGueltigeDauer(txtDauer.Text))
            {
                MessageBox.Show(
                    "Bitte eine gültige Dauer (1-1440 Minuten) eingeben.");
                return;
            }

            double dauerMinuten = Namensvergleich.ZahlLesen(txtDauer.Text);

            MetEintrag ausgewaehlterMetEintrag = metEintraege[cboAktivitaet.SelectedIndex];

            double kalorien = fitnessRechner.BerechneKalorien(ausgewaehlterMetEintrag.MetWert, benutzerGewicht, dauerMinuten);

            FitnessEintrag neuerEintrag = new FitnessEintrag(DateTime.Now, ausgewaehlterMetEintrag.Aktivitaet, kalorien);

            try
            {
                FitnessEintrag.Speichere(neuerEintrag);

                MessageBox.Show(kalorien.ToString("0") + " kcal wurden gespeichert!\nDas entspricht " + dauerMinuten + " Minuten " + ausgewaehlterMetEintrag.Aktivitaet);

                AppLogger.Info("Fitness gespeichert: " + ausgewaehlterMetEintrag.Aktivitaet);

                LadeHeutigeAktivitaeten();
                txtDauer.Text = "30";
                cboAktivitaet.SelectedIndex = 0;
            }
            catch (IOException)
            {
                AppLogger.Error("Fehler beim Speichern des Fitnesseintrags");
                MessageBox.Show("Fehler beim Speichern.");
            }
        }

        private void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            int index = lstAktivitaeten.SelectedIndex;

            if (index < 0 || index >= heutigeAktivitaeten.Count)
            {
                MessageBox.Show("Bitte zuerst eine Aktivität auswählen.");
                return;
            }

            if (MessageBox.Show("Möchtest du die ausgewählte Aktivität wirklich löschen?", "Aktivität löschen",
                MessageBoxButton.YesNo, //war ein random video auf meiner fy und wollte es direkt ausprobieren
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                bool geloescht = FitnessEintrag.LoescheVonTagNachIndex(DateTime.Now, index);

                if (!geloescht)
                {
                    MessageBox.Show("Die Aktivität konnte nicht gefunden werden.");
                    return;
                }

                LadeHeutigeAktivitaeten();
                AppLogger.Info("Fitnessaktivität gelöscht");
            }
            catch (IOException)
            {
                AppLogger.Error("Fehler beim Löschen der Fitnessaktivität");
                MessageBox.Show("Die Aktivität konnte nicht gelöscht werden.");
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
            Close();
        }
    }
}
