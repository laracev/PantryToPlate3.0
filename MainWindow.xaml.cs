using PantryToPlate;
using PantryToPlate.Models;
using PantryToPlate.Usercontrols;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using PantryToPlate.logik;

namespace PTP
{
    public partial class MainWindow : Window
    {
        private List<string> mahlzeitenHeuteNamen = new List<string>();
        private List<double> mahlzeitenHeuteKalorien = new List<double>();
        private double kalorienZiel = 2000;
        private double gegesseneKalorien = 0;
        private double verbrannteKalorien = 0;
        private double proteineHeute = 0;
        private double kohlenhydrateHeute = 0;
        private double fettHeute = 0;
        private KalorienRechner kalorienRechner = new KalorienRechner();
        private RezeptRechner rezeptRechner = new RezeptRechner();
        private DispatcherTimer updateTimer;

        public MainWindow()
        {
            InitializeComponent();
            heuteGegessenAnzeigen.MahlzeitLoeschenAngefordert += MahlzeitLoeschen;
            LadeAlleDaten();
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromSeconds(60);
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            LadeHeutigeMahlzeiten();
            LadeHeutigeFitness();
            AktualisiereAnzeige();
            ZeigeBestesRezept();
        }

        private void LadeAlleDaten()
        {
            try
            {
                LadeBenutzerDaten();
                LadeHeutigeMahlzeiten();
                LadeHeutigeFitness();
                AktualisiereAnzeige();
                ZeigeBestesRezept();
            }
            catch
            {
                AppLogger.Error("Fehler in LadeAlleDaten");
            }
        }

        private void LadeBenutzerDaten()
        {
            Benutzer benutzer = Benutzer.LadeAusCsv();
            kalorienZiel = (benutzer != null) ? benutzer.KalorienZiel : 2000;
        }

        private void LadePantryDaten()
        {
            try
            {
                AppDaten.SetzePantry(PantryItem.LadeAlleAusCsv());
            }
            catch
            {
                AppLogger.Error("Fehler beim Laden der Pantry-Daten");
            }
        }

        private void LadeRezeptDaten()
        {
            try
            {
                AppDaten.SetzeRezepte(Rezept.LadeAlleAusCsv());
            }
            catch
            {
                AppLogger.Error("Fehler beim Laden der Rezept-Daten");
            }
        }

        private void LadeHeutigeMahlzeiten()
        {
            List<MahlzeitEintrag> mahlzeiten = MahlzeitEintrag.LadeVonTag(DateTime.Now);
            mahlzeitenHeuteNamen.Clear();
            mahlzeitenHeuteKalorien.Clear();
            foreach (MahlzeitEintrag m in mahlzeiten)
            {
                mahlzeitenHeuteNamen.Add(m.Name);
                mahlzeitenHeuteKalorien.Add(m.Kalorien);
            }


            gegesseneKalorien = kalorienRechner.BerechneGegesseneKalorien(mahlzeiten);
            proteineHeute = kalorienRechner.BerechneProteine(mahlzeiten);
            kohlenhydrateHeute = kalorienRechner.BerechneKohlenhydrate(mahlzeiten);
            fettHeute = kalorienRechner.BerechneFett(mahlzeiten);
            heuteGegessenAnzeigen.SetzeMahlzeiten(mahlzeitenHeuteNamen, mahlzeitenHeuteKalorien);
        }

        private void LadeHeutigeFitness()
        {
            List<FitnessEintrag> eintraege = FitnessEintrag.LadeVonTag(DateTime.Now);
            verbrannteKalorien = 0;
            foreach (FitnessEintrag f in eintraege)
            {
                verbrannteKalorien += f.VerbrannteKalorien;
            }
        }

        private void AktualisiereAnzeige()
        {
            double netto = kalorienRechner.BerechneNettoKalorien(gegesseneKalorien, verbrannteKalorien);
            double uebrig = kalorienRechner.BerechneUebrigeKalorien(kalorienZiel, netto);
            double ueberschuss = kalorienRechner.BerechneUeberschussKalorien(kalorienZiel, netto);

            txtKalorienZiel.Text = kalorienZiel.ToString("0") + " kcal";
            txtGegesseneKalorien.Text = gegesseneKalorien.ToString("0") + " kcal";
            txtVerbrannteKalorien.Text = verbrannteKalorien.ToString("0") + " kcal";

            if (ueberschuss > 0)
            {
                txtUebrigeKalorien.Text = "Überschuss: " + ueberschuss.ToString("0") + " kcal";
                kalorienStatusBorder.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            }
            else
            {
                txtUebrigeKalorien.Text = "Übrig: " + uebrig.ToString("0") + " kcal";
                kalorienStatusBorder.Background = (Brush)FindResource("SuccessColor");
            }

            txtProteine.Text = proteineHeute.ToString("0") + " g";
            txtKohlenhydrate.Text = kohlenhydrateHeute.ToString("0") + " g";
            txtFett.Text = fettHeute.ToString("0") + " g";
            kalorienControl.SetzeFortschritt(netto, kalorienZiel);
        }

        private void ZeigeBestesRezept()
        {
            if (AppDaten.Rezepte == null || AppDaten.Rezepte.Count == 0)
            {
                rezeptVorschlag.SetzeBestesRezept(null);
                return;
            }

            rezeptRechner.AktualisiereMatches(AppDaten.Rezepte, AppDaten.Pantry);

            Rezept bestesRezept = AppDaten.Rezepte[0];
            for (int i = 1; i < AppDaten.Rezepte.Count; i++)
            {
                if (AppDaten.Rezepte[i].MatchProzent > bestesRezept.MatchProzent)
                {
                    bestesRezept = AppDaten.Rezepte[i];
                }
            }

            rezeptVorschlag.SetzeBestesRezept(bestesRezept);
        }

        private void btnEinstellungen_Click(object sender, RoutedEventArgs e)
        {
            Window1 fenster = new Window1();
            bool? wurdeGespeichert = fenster.ShowDialog();

            if (wurdeGespeichert == true)
            {
                kalorienZiel = fenster.GespeichertesKalorienZiel;
                AktualisiereAnzeige();
            }
        }

        private void btnMahlzeitHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            MahlzeitHinzufuegenWindow fenster = new MahlzeitHinzufuegenWindow();
            fenster.ShowDialog();
            LadeHeutigeMahlzeiten();
            AktualisiereAnzeige();
        }

        private void btnPantry_Click(object sender, RoutedEventArgs e)
        {
            PantryWindow fenster = new PantryWindow();
            fenster.ShowDialog();
            LadePantryDaten();
            LadeHeutigeMahlzeiten();
            AktualisiereAnzeige();
            ZeigeBestesRezept();
        }

        private void btnRezepte_Click(object sender, RoutedEventArgs e)
        {
            RezepteWindow fenster = new RezepteWindow();
            fenster.ShowDialog();
            LadeRezeptDaten();
            LadePantryDaten();
            LadeHeutigeMahlzeiten();
            AktualisiereAnzeige();
            ZeigeBestesRezept();
        }

        private void btnFitness_Click(object sender, RoutedEventArgs e)
        {
            FitnessWindow fenster = new FitnessWindow();
            fenster.ShowDialog();
            LadeHeutigeFitness();
            AktualisiereAnzeige();
        }

        private void btnEinkaufsliste_Click(object sender, RoutedEventArgs e)
        {
            EinkaufslisteWindow fenster = new EinkaufslisteWindow();
            fenster.ShowDialog();
            LadePantryDaten();
            ZeigeBestesRezept();
        }

        private void MahlzeitLoeschen(int index)
        {
            if (MessageBox.Show("Möchtest du diese Mahlzeit wirklich löschen?", "Mahlzeit löschen", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                bool geloescht = MahlzeitEintrag.LoescheVonTagNachIndex(DateTime.Now, index);

                if (!geloescht)
                {
                    MessageBox.Show("Die Mahlzeit konnte nicht gefunden werden.");
                    return;
                }

                LadeHeutigeMahlzeiten();
                AktualisiereAnzeige();
                AppLogger.Info("Mahlzeit gelöscht");
            }
            catch
            {
                AppLogger.Error("Fehler beim Löschen einer Mahlzeit");
                MessageBox.Show("Die Mahlzeit konnte nicht gelöscht werden.");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (updateTimer != null)
            {
                updateTimer.Stop();
            }
            base.OnClosed(e);
        }
    }
}
