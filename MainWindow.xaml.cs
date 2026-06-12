using PantryToPlate;
using PantryToPlate.Models;
using PantryToPlate.Usercontrols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace PTP
{
    public partial class MainWindow : Window
    {
        private string benutzerDatei = "data/benutzer.csv";
        private string mahlzeitenDatei = "data/tagesmahlzeiten.csv";
        private string fitnessDatei = "data/FitnessEintraege.csv";
        private string rezepteDatei = "data/rezepte.csv";
        private string pantryDatei = "data/pantry.csv";
        private string lebensmittelDatei = "data/test_utf8.csv";
        private List<string> mahlzeitenHeuteNamen = new List<string>();
        private List<double> mahlzeitenHeuteKalorien = new List<double>();
        private double kalorienZiel = 2000;
        private double gegesseneKalorien = 0;
        private double verbrannteKalorien = 0;
        private double proteineHeute = 0;
        private double kohlenhydrateHeute = 0;
        private double fettHeute = 0;

        private List<Rezept> alleRezepte = new List<Rezept>();
        private List<PantryItem> pantryItems = new List<PantryItem>();
        private Dictionary<string, double> lebensmittelKalorien = new Dictionary<string, double>();

        private DispatcherTimer updateTimer;

        public MainWindow()
        {
            InitializeComponent();

            LadeAlleDaten();

            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromSeconds(60);
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
            Logger.Info("MainWindow initialisiert");
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

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            LadeHeutigeMahlzeiten();
            LadeHeutigeFitness();
            AktualisiereAnzeige();
            Logger.Info("Automatische Aktualisierung der Daten");
        }

        private void LadeAlleDaten()
        {
            try
            {
                LadeLebensmittelDatenbank();
                LadeBenutzerDaten();
                LadePantry();
                LadeRezepte();
                LadeHeutigeMahlzeiten();
                LadeHeutigeFitness();
                AktualisiereAnzeige();
                ZeigeBestesRezept();
                Logger.Info("Daten erfolgreich geladen");
            }
            catch
            {
                Logger.Fehler("Fehler beim Laden der Daten");
            }
        }

        private void LadeLebensmittelDatenbank()
        {
            lebensmittelKalorien.Clear();

            if (File.Exists(lebensmittelDatei))
            {
                string[] zeilen = File.ReadAllLines(lebensmittelDatei);
                for (int i = 1; i < zeilen.Length; i++)
                {
                    string[] teile = zeilen[i].Split(';');
                    if (teile.Length >= 5)
                    {
                        try
                        {
                            string name = teile[0].Trim().ToLower();
                            double kalorien = ZahlLesen(teile[1]);

                            if (!lebensmittelKalorien.ContainsKey(name))
                            {
                                lebensmittelKalorien.Add(name, kalorien);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private void LadeBenutzerDaten()
        {
            try
            {
                if (File.Exists(benutzerDatei))
                {
                    string[] zeilen = File.ReadAllLines(benutzerDatei);
                    if (zeilen.Length > 1)
                    {
                        string[] teile = zeilen[1].Split(';');
                        if (teile.Length >= 1)
                        {
                            kalorienZiel = ZahlLesen(teile[0]);
                        }
                    }
                }
            }
            catch
            {
                Logger.Fehler("Fehler beim Laden der Benutzerdaten");
            }

            if (kalorienZiel <= 0) kalorienZiel = 2000;
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
                        double menge = ZahlLesen(teile[1]);

                        PantryItem item = new PantryItem();
                        item.Name = teile[0];
                        item.Menge = menge;
                        pantryItems.Add(item);
                    }
                }
            }
        }

        private void LadeRezepte()
        {
            alleRezepte.Clear();

            if (!File.Exists(rezepteDatei))
            {
                return;
            }

            string[] zeilen = File.ReadAllLines(rezepteDatei);

            int maxRezepte = zeilen.Length;


            for (int i = 1; i < maxRezepte; i++)
            {
                string zeile = zeilen[i];
                if (string.IsNullOrWhiteSpace(zeile))
                {
                    continue;
                }

                Rezept r = ParseRezept(zeile);
                if (r != null && r.Zutaten.Count > 0)
                {
                    BerechneRezeptKalorien(r);
                    BerechneMatchProzent(r);
                    alleRezepte.Add(r);
                }
            }

            for (int i = 0; i < alleRezepte.Count - 1; i++)
            {
                for (int j = i + 1; j < alleRezepte.Count; j++)
                {
                    if (alleRezepte[i].MatchProzent < alleRezepte[j].MatchProzent)
                    {
                        Rezept temp = alleRezepte[i];
                        alleRezepte[i] = alleRezepte[j];
                        alleRezepte[j] = temp;
                    }
                }
            }
        }

        private Rezept ParseRezept(string zeile)
        {
            try
            {
                string[] teile = zeile.Split(';');
                if (teile.Length < 3)
                {
                    return null;
                }

                Rezept r = new Rezept();
                r.Name = teile[0].Trim();

                if (string.IsNullOrWhiteSpace(r.Name))
                {
                    return null;
                }

                // Anleitung
                if (teile.Length >= 4)
                {
                    r.Anleitung = teile[3].Trim();
                    r.Anleitung = r.Anleitung.Replace("<br>", "\n");
                    r.Anleitung = r.Anleitung.Replace("<br/>", "\n");
                    r.Anleitung = r.Anleitung.Replace("</p><p>", "\n\n");
                    r.Anleitung = r.Anleitung.Replace("<p>", "");
                    r.Anleitung = r.Anleitung.Replace("</p>", "");
                }
                else
                {
                    r.Anleitung = "Keine Anleitung verfügbar.";
                }

                string zutatenString = teile[2].Trim();
                string[] zutatenMitMenge = zutatenString.Split('|');

                for (int j = 0; j < zutatenMitMenge.Length && j < 20; j++)
                {

                    string zutatBlock = zutatenMitMenge[j];
                    if (zutatBlock.Contains(":"))
                    {
                        int letzterDoppelpunkt = zutatBlock.LastIndexOf(':');
                        string zutatName = zutatBlock.Substring(0, letzterDoppelpunkt);
                        string mengeText = zutatBlock.Substring(letzterDoppelpunkt + 1);

                        double menge = ZahlLesen(mengeText);

                        zutatName = zutatName.Trim();

                       

                        if (zutatName.Length > 0 && zutatName.Length < 60 && menge > 0 && menge < 2000)
                        {
                            r.Zutaten.Add(zutatName);
                            r.ZutatenMengen.Add(menge);
                        }
                    }
                }
                return r;
            }
            catch
            {
                return null;
            }
        }


        private void BerechneRezeptKalorien(Rezept r)
        {
            double gesamt = 0;
            for (int i = 0; i < r.Zutaten.Count; i++)
            {
                string zutat = r.Zutaten[i].ToLower();
                double kalorienPro100g = 100;

                if (lebensmittelKalorien.ContainsKey(zutat))
                {
                    kalorienPro100g = lebensmittelKalorien[zutat];
                }
                else
                {
                    bool gefunden = false;
                    foreach (string key in lebensmittelKalorien.Keys)
                    {
                        if (key.Contains(zutat) || zutat.Contains(key))
                        {
                            kalorienPro100g = lebensmittelKalorien[key];
                            gefunden = true;
                            break;
                        }
                    }
                }

                gesamt = gesamt + (kalorienPro100g * r.ZutatenMengen[i]) / 100.0;
            }

            if (gesamt > 0)
            {
                r.KalorienProPortion = gesamt;
            }
            else
            {
                r.KalorienProPortion = 400;
            }
        }

        private void BerechneMatchProzent(Rezept r)
        {
            int vorhanden = 0;
            for (int i = 0; i < r.Zutaten.Count; i++)
            {
                string zutatLower = r.Zutaten[i].ToLower();
                bool gefunden = false;

                for (int j = 0; j < pantryItems.Count; j++)
                {
                    if (pantryItems[j].Name.ToLower().Contains(zutatLower) && pantryItems[j].Menge >= r.ZutatenMengen[i])
                    {
                        gefunden = true;
                        break;
                    }
                }

                if (gefunden)
                {
                    vorhanden = vorhanden + 1;
                }
            }

            if (r.Zutaten.Count > 0)
            {
                r.MatchProzent = (vorhanden * 100) / r.Zutaten.Count;
            }
            else
            {
                r.MatchProzent = 0;
            }
        }

        private void ZeigeBestesRezept()
        {
            Rezept bestesRezept = null;

            for (int i = 0; i < alleRezepte.Count; i++)
            {
                if (alleRezepte[i].MatchProzent > 0)
                {
                    bestesRezept = alleRezepte[i];
                    break;
                }
            }

            if (bestesRezept == null && alleRezepte.Count > 0)
            {
                bestesRezept = alleRezepte[0];
            }

            rezeptVorschlag.SetzeBestesRezept(bestesRezept);
        }

        private void LadeHeutigeMahlzeiten()
        {
            gegesseneKalorien = 0;
            proteineHeute = 0;
            kohlenhydrateHeute = 0;
            fettHeute = 0;

            mahlzeitenHeuteNamen.Clear();
            mahlzeitenHeuteKalorien.Clear();

            string heute = DateTime.Now.ToString("yyyy-MM-dd");

            try
            {
                if (File.Exists(mahlzeitenDatei))
                {
                    string[] zeilen = File.ReadAllLines(mahlzeitenDatei);

                    for (int i = 1; i < zeilen.Length; i++)
                    {
                        string[] teile = zeilen[i].Split(';');

                        if (teile.Length >= 7 && teile[0] == heute)
                        {
                            string name = teile[1];
                            double kalorien = ZahlLesen(teile[3]);
                            double proteine = ZahlLesen(teile[4]);
                            double kohlen = ZahlLesen(teile[5]);
                            double fettWert = ZahlLesen(teile[6]);

                            gegesseneKalorien += kalorien;
                            proteineHeute += proteine;
                            kohlenhydrateHeute += kohlen;
                            fettHeute += fettWert;

                            mahlzeitenHeuteNamen.Add(name);
                            mahlzeitenHeuteKalorien.Add(kalorien);
                        }
                    }
                }

                heuteGegessenAnzeigen.SetzeMahlzeiten(mahlzeitenHeuteNamen, mahlzeitenHeuteKalorien);
            }
            catch
            {
                Logger.Fehler("Fehler beim Laden der Mahlzeiten");
            }
        }
        private void LadeHeutigeFitness()
        {
            verbrannteKalorien = 0;
            string heute = DateTime.Now.ToString("yyyy-MM-dd");

            try
            {
                if (File.Exists(fitnessDatei))
                {
                    string[] zeilen = File.ReadAllLines(fitnessDatei);
                    for (int i = 1; i < zeilen.Length; i++)
                    {
                        string[] teile = zeilen[i].Split(';');
                        if (teile.Length >= 3 && teile[0] == heute)
                        {
                            double kalorien = ZahlLesen(teile[2]);
                            verbrannteKalorien += kalorien;
                        }
                    }
                }
            }
            catch
            {
                Logger.Fehler("Fehler beim Laden der Fitnessdaten");
            }
        }

        private void AktualisiereAnzeige()
        {
            double nettoKalorien = gegesseneKalorien - verbrannteKalorien;
            if (nettoKalorien < 0) nettoKalorien = 0;

            double uebrige = kalorienZiel - nettoKalorien;
            if (uebrige < 0) uebrige = 0;

            txtKalorienZiel.Text = kalorienZiel.ToString("0") + " kcal";
            txtGegesseneKalorien.Text = gegesseneKalorien.ToString("0") + " kcal";
            txtVerbrannteKalorien.Text = verbrannteKalorien.ToString("0") + " kcal";
            txtUebrigeKalorien.Text = "Übrig: " + uebrige.ToString("0") + " kcal";

            txtProteine.Text = proteineHeute.ToString("0") + " g";
            txtKohlenhydrate.Text = kohlenhydrateHeute.ToString("0") + " g";
            txtFett.Text = fettHeute.ToString("0") + " g";

            kalorienControl.SetzeFortschritt(nettoKalorien, kalorienZiel);
            kalorienControl.SetzeBeschreibung("Kalorien-Fortschritt: " + nettoKalorien.ToString("0") + " / " + kalorienZiel.ToString("0") + " kcal");
        }

        private void btnEinstellungen_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("Einstellungen-Fenster geöffnet");
            Window1 fenster = new Window1();
            fenster.ShowDialog();
            LadeBenutzerDaten();
            AktualisiereAnzeige();
        }

        private void btnMahlzeitHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("MahlzeitHinzufuegen-Fenster geöffnet");
            MahlzeitHinzufuegenWindow fenster = new MahlzeitHinzufuegenWindow();
            fenster.ShowDialog();
            LadeHeutigeMahlzeiten();
            AktualisiereAnzeige();
        }

        private void btnPantry_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("Pantry-Fenster geöffnet");
            PantryWindow fenster = new PantryWindow();
            fenster.ShowDialog();
            LadePantry();
            LadeRezepte();
            ZeigeBestesRezept();
        }

        private void btnRezepte_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("Rezepte-Fenster geöffnet");
            LadePantry();
            LadeRezepte();

            RezepteWindow fenster = new RezepteWindow();
            fenster.ShowDialog();
            LadeHeutigeMahlzeiten();
            AktualisiereAnzeige();
        }

        private void btnFitness_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("Fitness-Fenster geöffnet");
            FitnessWindow fenster = new FitnessWindow();
            fenster.ShowDialog();
            LadeHeutigeFitness();
            AktualisiereAnzeige();
        }

        private void btnEinkaufsliste_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("Einkaufsliste-Fenster geöffnet");
            EinkaufslisteWindow fenster = new EinkaufslisteWindow();
            fenster.ShowDialog();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (updateTimer != null)
            {
                updateTimer.Stop();
            }
            Logger.Info("Anwendung geschlossen");
            base.OnClosed(e);
        }

        private void heuteGegessenAnzeigen_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void rezeptVorschlag_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}