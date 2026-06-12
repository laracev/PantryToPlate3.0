using PantryToPlate.Models;
using PTP;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PantryToPlate
{
    public partial class SplashScreenWindow : Window
    {
        private DispatcherTimer ladetimer;
        private int ladestatus = 0;

        private List<Lebensmittel> vorgeladeneLebensmittel = new List<Lebensmittel>();
        private List<Rezept> vorgeladeneRezepte = new List<Rezept>();
        private List<PantryItem> vorgeladenePantry = new List<PantryItem>();
        private List<MetEintrag> vorgeladeneMetEintraege = new List<MetEintrag>();
        private Dictionary<string, double> vorgeladeneKalorien = new Dictionary<string, double>();

        private string[] statusTexte = new string[]
        {
            "Initialisiere...",
            "Lade Lebensmitteldatenbank (5000+ Einträge)...",
            "Lade Benutzerdaten...",
            "Lade Vorratsdaten...",
            "Lade Rezepte (1200+ Rezepte)...",
            "Lade Fitness-Datenbank...",
            "Starte Anwendung..."
        };

        public SplashScreenWindow()
        {
            InitializeComponent();
            this.Loaded += SplashScreenWindow_Loaded;
            
        }

        private void SplashScreenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartePunktAnimation();
            StarteLadevorgang();
        }

        private void StartePunktAnimation()
        {
            var dots = new[] { dot1, dot2, dot3, dot4 };

            for (int i = 0; i < dots.Length; i++)
            {
                DoubleAnimation animation = new DoubleAnimation();
                animation.From = 0.5;
                animation.To = 1.5;
                animation.Duration = TimeSpan.FromSeconds(0.5);
                animation.AutoReverse = true;
                animation.RepeatBehavior = RepeatBehavior.Forever;
                animation.BeginTime = TimeSpan.FromMilliseconds(i * 150);

                dots[i].RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                dots[i].RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            }
        }

        private void StarteLadevorgang()
        {
            ladetimer = new DispatcherTimer();
            ladetimer.Interval = TimeSpan.FromMilliseconds(700);
            ladetimer.Tick += Ladetimer_Tick;
            ladetimer.Start();

            DoubleAnimation fortschrittAnim = new DoubleAnimation();
            fortschrittAnim.From = 0;
            fortschrittAnim.To = 400;
            fortschrittAnim.Duration = TimeSpan.FromSeconds(4.9);
            progressBar.BeginAnimation(Border.WidthProperty, fortschrittAnim);
        }

        private void Ladetimer_Tick(object sender, EventArgs e)
        {
            if (ladestatus < statusTexte.Length)
            {
                txtStatus.Text = statusTexte[ladestatus];
                Logger.Info(statusTexte[ladestatus]);

                switch (ladestatus)
                {
                    case 0: // Initialisiere
                        Directory.CreateDirectory("data");
                        break;
                    case 1: // Lade Lebensmitteldatenbank KOMPLETT
                        LadeLebensmittelDatenbankKomplett();
                        break;
                    case 2: // Lade Benutzerdaten
                        LadeBenutzerDaten();
                        break;
                    case 3: // Lade Vorratsdaten
                        LadeVorratsDatenKomplett();
                        break;
                    case 4: // Lade Rezepte KOMPLETT
                        LadeRezepteDatenKomplett();
                        break;
                    case 5: // Lade MET Tabelle
                        LadeMetTabelleKomplett();
                        break;
                }

                ladestatus++;
            }
            else
            {
                ladetimer.Stop();

                SpeichereVorgeladeneDaten();

                DispatcherTimer schliessTimer = new DispatcherTimer();
                schliessTimer.Interval = TimeSpan.FromSeconds(0.5);
                schliessTimer.Tick += (s, args) =>
                {
                    schliessTimer.Stop();
                    Logger.Info("Ladevorgang abgeschlossen, Hauptfenster wird geöffnet");
                    Logger.Info("Geladene Daten: " + vorgeladeneLebensmittel.Count + " Lebensmittel, " + vorgeladeneRezepte.Count + " Rezepte");

                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                    AppDaten.Lebensmittel = vorgeladeneLebensmittel;
                    AppDaten.Rezepte = vorgeladeneRezepte;
                    AppDaten.Pantry = vorgeladenePantry;
                    AppDaten.MetEintraege = vorgeladeneMetEintraege;
                    AppDaten.LebensmittelKalorien = vorgeladeneKalorien;
                    AppDaten.IstGeladen = true;
                };
                schliessTimer.Start();
            }
        }

        private void LadeLebensmittelDatenbankKomplett()
        {
            string datei = "data/test_utf8.csv";
            vorgeladeneLebensmittel.Clear();
            vorgeladeneKalorien.Clear();

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
                        string name = teile[0].Trim().ToLower();
                        double kalorien = Helper.ToDouble(teile[1]);

                        if (!vorgeladeneKalorien.ContainsKey(name))
                        {
                            vorgeladeneKalorien.Add(name, kalorien);
                        }

                        Lebensmittel lm = new Lebensmittel();
                        lm.Name = teile[0].Trim();
                        lm.KalorienPro100g = kalorien;
                        lm.ProteinePro100g = Helper.ToDouble(teile[2]);
                        lm.FettPro100g = Helper.ToDouble(teile[3]);
                        lm.KohlenhydratePro100g = Helper.ToDouble(teile[4]);

                        if (teile.Length >= 6)
                        {
                            lm.BallaststoffePro100g = Helper.ToDouble(teile[5]);
                        }

                        vorgeladeneLebensmittel.Add(lm);
                    }
                }
            }

            Logger.Info(vorgeladeneLebensmittel.Count + " Lebensmittel geladen");
        }

        private void LadeBenutzerDaten()
        {
            string datei = "data/benutzer.csv";
            if (File.Exists(datei))
            {
                string[] zeilen = File.ReadAllLines(datei);
                Logger.Info("Benutzerdaten geladen");
            }
        }

        private void LadeVorratsDatenKomplett()
        {
            string datei = "data/pantry.csv";
            vorgeladenePantry.Clear();

            if (File.Exists(datei))
            {
                string[] zeilen = File.ReadAllLines(datei);
                for (int i = 1; i < zeilen.Length; i++)
                {
                    string[] teile = zeilen[i].Split(';');
                    if (teile.Length >= 2)
                    {
                        PantryItem item = new PantryItem();
                        item.Name = teile[0];
                        item.Menge = Helper.ToDouble(teile[1]);
                        vorgeladenePantry.Add(item);
                    }
                }
            }

            Logger.Info(vorgeladenePantry.Count + " Vorratsartikel geladen");
        }

        private void LadeRezepteDatenKomplett()
        {
            string datei = "data/rezepte.csv";
            vorgeladeneRezepte.Clear();

            if (File.Exists(datei))
            {
                string[] zeilen = File.ReadAllLines(datei);

                for (int i = 1; i < zeilen.Length; i++)
                {
                    string zeile = zeilen[i];
                    if (string.IsNullOrWhiteSpace(zeile)) continue;

                    Rezept r = ParseRezeptFuerSplash(zeile);
                    if (r != null && r.Zutaten.Count > 0)
                    {
                        BerechneRezeptKalorienFuerSplash(r);
                        vorgeladeneRezepte.Add(r);
                    }
                }
            }

            Logger.Info(vorgeladeneRezepte.Count + " Rezepte geladen");
        }

        private void LadeMetTabelleKomplett()
        {
            string datei = "data/MET_Werte_Tabelle.csv";
            vorgeladeneMetEintraege.Clear();

            if (File.Exists(datei))
            {
                string[] zeilen = File.ReadAllLines(datei);

                for (int i = 1; i < zeilen.Length; i++)
                {
                    string zeile = zeilen[i];
                    if (string.IsNullOrWhiteSpace(zeile)) continue;

                    string[] teile = zeile.Split(';');
                    if (teile.Length >= 2)
                    {
                        MetEintrag met = new MetEintrag();
                        met.Aktivitaet = teile[0].Trim();

                        string metText = teile[1].Trim();
                        metText = metText.Replace(',', '.');
                        double.TryParse(metText, out double metWert);
                        met.MetWert = metWert;

                        vorgeladeneMetEintraege.Add(met);
                    }
                }
            }

            if (vorgeladeneMetEintraege.Count == 0)
            {
                MetEintrag m1 = new MetEintrag();
                m1.Aktivitaet = "Gehen (langsam)";
                m1.MetWert = 2.0;
                vorgeladeneMetEintraege.Add(m1);

                MetEintrag m2 = new MetEintrag();
                m2.Aktivitaet = "Joggen";
                m2.MetWert = 7.0;
                vorgeladeneMetEintraege.Add(m2);
            }

            Logger.Info(vorgeladeneMetEintraege.Count + " MET-Einträge geladen");
        }

        private Rezept ParseRezeptFuerSplash(string zeile)
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

                if (teile.Length >= 4)
                {
                    r.Anleitung = teile[3].Trim();
                    r.Anleitung = r.Anleitung.Replace("<br>", "\n");
                    r.Anleitung = r.Anleitung.Replace("<br/>", "\n");
                }
                else
                {
                    r.Anleitung = "Keine Anleitung verfügbar.";
                }

                string zutatenString = teile[2].Trim();
                string[] zutatenMitMenge = zutatenString.Split('|');

                for (int j = 0; j < zutatenMitMenge.Length && j < 25; j++)
                {
                    string zutatBlock = zutatenMitMenge[j];
                    if (zutatBlock.Contains(":"))
                    {
                        int letzterDoppelpunkt = zutatBlock.LastIndexOf(':');
                        string zutatName = zutatBlock.Substring(0, letzterDoppelpunkt);
                        string mengeText = zutatBlock.Substring(letzterDoppelpunkt + 1);
                        double menge = Helper.ToDouble(mengeText);

                        zutatName = zutatName.Replace(" roh", "");
                        zutatName = zutatName.Replace(", roh", "");
                        zutatName = zutatName.Trim();

                        if (zutatName.Contains("/"))
                        {
                            string[] parts = zutatName.Split('/');
                            zutatName = parts[0].Trim();
                        }

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

        private void BerechneRezeptKalorienFuerSplash(Rezept r)
        {
            double gesamt = 0;
            for (int i = 0; i < r.Zutaten.Count; i++)
            {
                string zutat = r.Zutaten[i].ToLower();
                double kalorienPro100g = 100;

                if (vorgeladeneKalorien.ContainsKey(zutat))
                {
                    kalorienPro100g = vorgeladeneKalorien[zutat];
                }
                else
                {
                    foreach (string key in vorgeladeneKalorien.Keys)
                    {
                        if (key.Contains(zutat) || zutat.Contains(key))
                        {
                            kalorienPro100g = vorgeladeneKalorien[key];
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

        private void SpeichereVorgeladeneDaten()
        {
            string zwischenDatei = "data/vorgeladene_daten.txt";
            string inhalt = "Vorgeladene Daten:\n";
            inhalt = inhalt + "Lebensmittel: " + vorgeladeneLebensmittel.Count + "\n";
            inhalt = inhalt + "Rezepte: " + vorgeladeneRezepte.Count + "\n";
            inhalt = inhalt + "Pantry: " + vorgeladenePantry.Count + "\n";
            inhalt = inhalt + "MET: " + vorgeladeneMetEintraege.Count + "\n";
            File.WriteAllText(zwischenDatei, inhalt);
        }
    }
}