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

        //hab das ganze auf github gefunden also so eine animation vorlage hab einfach nur bestimmte dinge umgeändert und so
        private DispatcherTimer ladetimer;
        private int ladestatus = 0;
        private bool ladenAbgeschlossen = false;
        private string[] statusTexte = new string[]
        {
            "Initialisiere...",
            "Lade Lebensmitteldatenbank...",
            "Lade Benutzerdaten...",
            "Lade Vorratsdaten...",
            "Lade Rezepte...",
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
            FrameworkElement[] dots = new FrameworkElement[] { dot1, dot2, dot3, dot4 };
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
                try
                {
                    switch (ladestatus)
                    {
                        case 0:
                            Directory.CreateDirectory("data");
                            break;
                        case 1:
                            LadeLebensmittel();
                            break;
                        case 2:
                            LadeBenutzer();
                            break;
                        case 3:
                            LadePantry();
                            break;
                        case 4:
                            LadeRezepte();
                            break;
                        case 5:
                            LadeMetEintraege();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.Error("Fehler im SplashScreen: " + ex.Message);
                }
                ladestatus++;
            }
            else if (!ladenAbgeschlossen)
            {
                ladenAbgeschlossen = true;
                ladetimer.Stop();
                AppDaten.IstGeladen = true;
                AppLogger.Info("Alle Daten geladen. Starte Hauptfenster.");
                DispatcherTimer schliessTimer = new DispatcherTimer();
                schliessTimer.Interval = TimeSpan.FromSeconds(0.5);
                schliessTimer.Tick += SchliessTimer_Tick;
                schliessTimer.Start();
            }
        }

        private void SchliessTimer_Tick(object sender, EventArgs e)
        {
            ((DispatcherTimer)sender).Stop();
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void LadeLebensmittel()
        {
            AppDaten.Lebensmittel = Lebensmittel.LadeAlleAusCsv();
            AppDaten.LebensmittelKalorien.Clear();
            foreach (Lebensmittel lm in AppDaten.Lebensmittel)
            {
                string nameLower = lm.Name.ToLower();
                if (!AppDaten.LebensmittelKalorien.ContainsKey(nameLower))
                {
                    AppDaten.LebensmittelKalorien.Add(nameLower, lm.KalorienPro100g);
                }
            }
        }

        private void LadeBenutzer()
        {
            Benutzer.LadeAusCsv();
        }

        private void LadePantry()
        {
            AppDaten.Pantry = PantryItem.LadeAlleAusCsv();
        }

        private void LadeRezepte()
        {
            AppDaten.Rezepte = Rezept.LadeAlleAusCsv();
            PantryToPlate.logik.RezeptRechner rechner = new PantryToPlate.logik.RezeptRechner();
            foreach (Rezept r in AppDaten.Rezepte)
            {
                r.KalorienProPortion = rechner.BerechneKalorien(r, AppDaten.LebensmittelKalorien);
                r.MatchProzent = rechner.BerechneMatch(r, AppDaten.Pantry);
            }
        }

        private void LadeMetEintraege()
        {
            AppDaten.MetEintraege = MetEintrag.LadeAlleAusCsv();
        }
    }
}