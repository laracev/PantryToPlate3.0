using PantryToPlate.logik;
using PantryToPlate.Models;
using PTP;
using System;
using System.Collections.Generic;
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
        private bool ladenAbgeschlossen = false;
        private bool ladefehlerAufgetreten = false;

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
            Loaded += SplashScreenWindow_Loaded;
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
                            Benutzer.LadeAusCsv();
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
                catch (IOException)
                {
                    ladefehlerAufgetreten = true;
                }

                ladestatus++;
            }
            else if (!ladenAbgeschlossen)
            {
                ladenAbgeschlossen = true;
                ladetimer.Stop();
                AppDaten.SetzeGeladen(!ladefehlerAufgetreten);

                if (ladefehlerAufgetreten)
                {
                    MessageBox.Show("Einige Daten konnten nicht geladen werden.");
                }

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
            Close();
        }

        private void LadeLebensmittel()
        {
            List<Lebensmittel> lebensmittel = Lebensmittel.LadeAlleAusCsv();
            AppDaten.SetzeLebensmittel(lebensmittel);
            AppDaten.LebensmittelKalorien.Clear();

            foreach (Lebensmittel eintrag in AppDaten.Lebensmittel)
            {
                string name = eintrag.Name.ToLower();
                if (!AppDaten.LebensmittelKalorien.ContainsKey(name))
                {
                    AppDaten.LebensmittelKalorien.Add(name, eintrag.KalorienPro100g);
                }
            }
        }

        private void LadePantry()
        {
            AppDaten.SetzePantry(PantryItem.LadeAlleAusCsv());
        }

        private void LadeRezepte()
        {
            List<Rezept> rezepte = Rezept.LadeAlleAusCsv();
            RezeptRechner rechner = new RezeptRechner();

            foreach (Rezept rezept in rezepte)
            {
                rezept.SetzeKalorienProPortion(
                    rechner.BerechneKalorien(rezept, AppDaten.LebensmittelKalorien));
                rezept.SetzeMatchProzent(
                    rechner.BerechneMatch(rezept, AppDaten.Pantry));
            }

            AppDaten.SetzeRezepte(rezepte);
        }

        private void LadeMetEintraege()
        {
            AppDaten.SetzeMetEintraege(MetEintrag.LadeAlleAusCsv());
        }
    }
}
