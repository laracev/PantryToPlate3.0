using PantryToPlate.helpers;
using PantryToPlate.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PantryToPlate.logik;

namespace PantryToPlate
{
    public partial class RezepteWindow : Window
    {
        private List<Rezept> alleRezepte = new List<Rezept>();
        private List<Rezept> gefilterteRezepte = new List<Rezept>();
        private List<PantryItem> pantryItems = new List<PantryItem>();
        private Dictionary<string, double> lebensmittelKalorien = new Dictionary<string, double>();
        private RezeptRechner rezeptRechner = new RezeptRechner();
        private Rezept aktuellesRezept = null;
        private string aktuelleKategorie = "Alle";
        private string aktuellerSuchtext = "";

        public RezepteWindow()
        {
            InitializeComponent();
            this.Loaded += RezepteWindow_Loaded;
            this.Activated += RezepteWindow_Activated;
        }

        private void RezepteWindow_Activated(object sender, EventArgs e)
        {
            try
            {
                LadePantry();
                foreach (Rezept r in alleRezepte)
                {
                    r.SetzeMatchProzent(rezeptRechner.BerechneMatch(r, pantryItems));
                }
                FiltereUndZeigeRezepte();
                if (aktuellesRezept != null)
                {
                    ZeigeRezeptDetails(aktuellesRezept);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler in RezepteWindow_Activated");
                MessageBox.Show("Fehler beim Aktualisieren der Rezepte.");
            }
        }

        private void RezepteWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= RezepteWindow_Loaded;
            cboKategorie.SelectionChanged += cboKategorie_SelectionChanged;
            txtSuche.TextChanged += txtSuche_TextChanged;
            LadeAlleDaten();
        }

        private void LadeAlleDaten()
        {
            try
            {
                LadeLebensmittel();
                LadePantry();
                LadeRezepte();
                FiltereUndZeigeRezepte();
                AppLogger.Info("Rezepte geladen: " + alleRezepte.Count);
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler in LadeAlleDaten");
                MessageBox.Show("Fehler beim Laden der Rezepte.");
            }
        }

        private void LadeLebensmittel()
        {
            lebensmittelKalorien = AppDaten.LebensmittelKalorien;
        }

        private void LadePantry()
        {
            pantryItems = AppDaten.Pantry;
        }

        private void LadeRezepte()
        {
            alleRezepte = AppDaten.Rezepte;

            foreach (Rezept rezept in alleRezepte)
            {
                double kalorien = rezeptRechner.BerechneKalorien(rezept, lebensmittelKalorien);
                int match = rezeptRechner.BerechneMatch(rezept, pantryItems);
                rezept.SetzeKalorienProPortion(kalorien);
                rezept.SetzeMatchProzent(match);
            }
        }

        private string HoleKategorieAusName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Sonstige";
            }
            string[] woerter = name.ToLower().Split(' ');
            if (woerter.Length == 0)
            {
                return "Sonstige";
            }
            string erstesWort = woerter[0];
            if (erstesWort == "pasta")
            {
                return "Pasta";
            }
            if (erstesWort == "bowl")
            {
                return "Bowl";
            }
            if (erstesWort == "normale")
            {
                return "Normale Pizza";
            }
            if (erstesWort == "vegane")
            {
                return "Vegane Pizza";
            }
            if (erstesWort == "curry")
            {
                return "Curry";
            }
            if (erstesWort == "suppe")
            {
                return "Suppe";
            }
            if (erstesWort == "auflauf")
            {
                return "Auflauf";
            }
            if (erstesWort == "omelett")
            {
                return "Omelett";
            }
            if (erstesWort == "wrap")
            {
                return "Wrap";
            }
            if (erstesWort == "pfannengericht")
            {
                return "Pfannengericht";
            }
            if (erstesWort == "ofengericht")
            {
                return "Ofengericht";
            }
            if (erstesWort == "kuchen")
            {
                return "Kuchen";
            }
            if (erstesWort == "kekse")
            {
                return "Kekse";
            }
            if (erstesWort == "smoothie")
            {
                return "Smoothie";
            }
            if (erstesWort == "frühstück")
            {
                return "Frühstück";
            }
            if (erstesWort == "dessert")
            {
                return "Dessert";
            }
            return "Sonstige";
        }

        private void FiltereUndZeigeRezepte()
        {
            gefilterteRezepte.Clear();
            foreach (Rezept r in alleRezepte)
            {
                if (aktuelleKategorie != "Alle" && HoleKategorieAusName(r.Name) != aktuelleKategorie)
                {
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(aktuellerSuchtext) && !r.Name.ToLower().Contains(aktuellerSuchtext))
                {
                    continue;
                }
                gefilterteRezepte.Add(r);
            }
            for (int i = 0; i < gefilterteRezepte.Count - 1; i++)
            {
                for (int j = i + 1; j < gefilterteRezepte.Count; j++)
                {
                    if (gefilterteRezepte[i].MatchProzent < gefilterteRezepte[j].MatchProzent)
                    {
                        Rezept temp = gefilterteRezepte[i];
                        gefilterteRezepte[i] = gefilterteRezepte[j];
                        gefilterteRezepte[j] = temp;
                    }
                }
            }
            ZeigeRezeptListe();
        }

        private void ZeigeRezeptListe()
        {
            if (spRezeptListe == null)
            {
                return;
            }
            spRezeptListe.Children.Clear();
            int maxAnzeige = gefilterteRezepte.Count;
            if (maxAnzeige > 300)
            {
                maxAnzeige = 300;
            }
            for (int i = 0; i < maxAnzeige; i++)
            {
                Rezept r = gefilterteRezepte[i];
                string nameKurz = r.Name;
                if (nameKurz.Length > 55)
                {
                    nameKurz = nameKurz.Substring(0, 52) + "...";
                }
                Border border = new Border();
                border.SetResourceReference(Border.BackgroundProperty, "GeneratedListItemBackgroundBrush");
                border.CornerRadius = new CornerRadius(8);
                border.Padding = new Thickness(12, 10, 12, 10);
                border.Margin = new Thickness(0, 5, 0, 5);
                border.Cursor = System.Windows.Input.Cursors.Hand;
                border.Tag = i;
                border.MouseLeftButtonUp += RezeptElement_Click;
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                StackPanel sp = new StackPanel();
                Grid.SetColumn(sp, 1);
                TextBlock nameText = new TextBlock();
                nameText.Text = nameKurz;
                nameText.FontWeight = FontWeights.SemiBold;
                nameText.FontSize = 13;
                sp.Children.Add(nameText);
                TextBlock matchTextBlock = new TextBlock();
                matchTextBlock.Text = r.MatchProzent + "% verfügbar";
                matchTextBlock.FontSize = 10;
                matchTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "GeneratedMutedTextBrush");
                sp.Children.Add(matchTextBlock);
                grid.Children.Add(sp);
                Border prozentBorder = new Border();
                prozentBorder.SetResourceReference(Border.BackgroundProperty, "RecipePercentBrush");
                prozentBorder.CornerRadius = new CornerRadius(10);
                prozentBorder.Padding = new Thickness(8, 3, 8, 3);
                Grid.SetColumn(prozentBorder, 2);
                TextBlock prozentText = new TextBlock();
                prozentText.Text = r.MatchProzent + "%";
                prozentText.Foreground = new SolidColorBrush(Colors.White);
                prozentText.FontSize = 11;
                prozentText.FontWeight = FontWeights.Bold;
                prozentBorder.Child = prozentText;
                grid.Children.Add(prozentBorder);
                border.Child = grid;
                spRezeptListe.Children.Add(border);
            }
            if (gefilterteRezepte.Count == 0)
            {
                TextBlock emptyText = new TextBlock();
                emptyText.Text = "Keine Rezepte gefunden";
                emptyText.FontSize = 14;
                emptyText.SetResourceReference(TextBlock.ForegroundProperty, "GeneratedMutedTextBrush");
                emptyText.HorizontalAlignment = HorizontalAlignment.Center;
                emptyText.Margin = new Thickness(0, 30, 0, 0);
                spRezeptListe.Children.Add(emptyText);
            }
        }

        private void RezeptElement_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border != null && border.Tag is int index)
            {
                if (index >= 0 && index < gefilterteRezepte.Count)
                {
                    ZeigeRezeptDetails(gefilterteRezepte[index]);
                }
            }
        }

        private void ZeigeRezeptDetails(Rezept rezept)
        {
            if (rezept == null)
            {
                return;
            }
            aktuellesRezept = rezept;
            txtKeinRezept.Visibility = Visibility.Collapsed;
            spDetailsInhalt.Visibility = Visibility.Visible;
            txtRezeptName.Text = rezept.Name;
            txtKalorien.Text = rezept.KalorienProPortion.ToString("0") + " kcal";
            txtAnleitung.Text = rezept.Anleitung;
            spZutaten.Children.Clear();
            for (int i = 0; i < rezept.Zutaten.Count; i++)
            {
                Border zutatBorder = new Border();
                zutatBorder.SetResourceReference(Border.BackgroundProperty, "GeneratedListItemBackgroundBrush");
                zutatBorder.Padding = new Thickness(8, 5, 8, 5);
                zutatBorder.Margin = new Thickness(0, 2, 0, 2);
                zutatBorder.CornerRadius = new CornerRadius(6);
                TextBlock zutatText = new TextBlock();
                zutatText.FontSize = 12;
                zutatText.Text = rezept.Zutaten[i] + " (" + rezept.ZutatenMengen[i] + "g)";
                zutatBorder.Child = zutatText;
                spZutaten.Children.Add(zutatBorder);
            }
            btnFehlendeKaufen.Tag = rezeptRechner.ErmittleFehlendeZutaten(rezept, pantryItems);
            btnRezeptKochen.Tag = rezept;
        }

        private void cboKategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboKategorie.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                aktuelleKategorie = item.Tag.ToString();
                FiltereUndZeigeRezepte();
            }
        }

        private void txtSuche_TextChanged(object sender, TextChangedEventArgs e)
        {
            aktuellerSuchtext = (txtSuche.Text != null) ? txtSuche.Text.ToLower() : "";
            FiltereUndZeigeRezepte();
        }

        private double PantryMengeVon(string zutat)
        {
            double gesamt = 0;
            foreach (PantryItem item in pantryItems)
            {
                if (Namensvergleich.BerechneAehnlichkeit(zutat, item.Name) >= 120)
                {
                    gesamt += item.Menge;
                }
            }
            return gesamt;
        }

        private void ZieheAusPantryAb(string zutat, double benoetigt)
        {
            double rest = benoetigt;
            for (int i = 0; i < pantryItems.Count && rest > 0; i++)
            {
                if (Namensvergleich.BerechneAehnlichkeit(zutat, pantryItems[i].Name) >= 120)
                {
                    double abzug = pantryItems[i].Menge;
                    if (abzug > rest)
                    {
                        abzug = rest;
                    }
                    pantryItems[i].VerringereMenge(abzug);
                    rest -= abzug;
                }
            }
        }

        private void btnFehlendeKaufen_Click(object sender, RoutedEventArgs e)
        {
            if (aktuellesRezept == null)
            {
                MessageBox.Show("Bitte zuerst ein Rezept auswählen.");
                return;
            }
            string portionenText = Interaction.InputBox("Für wie viele Portionen möchtest du einkaufen?\n(1-5 Portionen möglich)", "Portionen für Einkaufsliste", "1");
            if (!int.TryParse(portionenText, out int portionen))
            {
                MessageBox.Show("Bitte eine ganze Zahl zwischen 1 und 5 eingeben.");
                return;
            }
            if (portionen < 1)
            {
                portionen = 1;
            }
            if (portionen > 5)
            {
                portionen = 5;
            }
            List<string> einkaufsListe = Einkaufsliste.Lade();
            List<string> neuHinzugefuegt = new List<string>();
            List<string> schonAufListe = new List<string>();
            for (int i = 0; i < aktuellesRezept.Zutaten.Count; i++)
            {
                string zutat = aktuellesRezept.Zutaten[i];
                double benoetigt = aktuellesRezept.ZutatenMengen[i] * portionen;
                double vorhanden = PantryMengeVon(zutat);
                double fehlt = benoetigt - vorhanden;
                if (fehlt <= 0)
                {
                    continue;
                }
                int fehltGerundet = (int)Math.Ceiling(fehlt);
                string neuerEintrag = zutat + " (" + fehltGerundet + "g)";
                bool gefundenAufListe = false;
                for (int j = 0; j < einkaufsListe.Count; j++)
                {
                    string vorhandenerName = Namensvergleich.NormalisiereName(einkaufsListe[j]);
                    if (Namensvergleich.BerechneAehnlichkeit(vorhandenerName, zutat) >= 120)
                    {
                        einkaufsListe[j] = neuerEintrag;
                        gefundenAufListe = true;
                        schonAufListe.Add(neuerEintrag);
                        break;
                    }
                }
                if (!gefundenAufListe)
                {
                    einkaufsListe.Add(neuerEintrag);
                    neuHinzugefuegt.Add(neuerEintrag);
                }
            }
            if (neuHinzugefuegt.Count == 0 && schonAufListe.Count == 0)
            {
                MessageBox.Show("Keine fehlenden Zutaten – alles ist bereits in der Pantry.");
                return;
            }
            Einkaufsliste.Speichere(einkaufsListe);
            string msg = "Fehlende Zutaten für " + portionen + " Portion(en):\n\n";
            if (neuHinzugefuegt.Count > 0)
            {
                msg += "Neu zur Einkaufsliste hinzugefügt:\n";
                foreach (string item in neuHinzugefuegt)
                {
                    msg += "• " + item + "\n";
                }
                msg += "\n";
            }
            if (schonAufListe.Count > 0)
            {
                msg += "Schon auf der Einkaufsliste / Menge aktualisiert:\n";
                foreach (string item in schonAufListe)
                {
                    msg += "• " + item + "\n";
                }
            }
            MessageBox.Show(msg, "Einkaufsliste aktualisiert", MessageBoxButton.OK, MessageBoxImage.Information);
            AppLogger.Info("Fehlende Zutaten für " + aktuellesRezept.Name + " zur Einkaufsliste hinzugefügt.");
        }

        private void btnRezeptKochen_Click(object sender, RoutedEventArgs e)
        {
            Rezept r = (Rezept)btnRezeptKochen.Tag;
            if (r == null)
            {
                return;
            }
            string portionenText = Interaction.InputBox("Wie viele Portionen möchtest du kochen?\n(1-5 Portionen möglich)", "Portionen auswählen", "1");
            if (!int.TryParse(portionenText, out int portionen))
            {
                MessageBox.Show("Bitte eine ganze Zahl zwischen 1 und 5 eingeben.");
                return;
            }
            if (portionen < 1)
            {
                portionen = 1;
            }
            if (portionen > 5)
            {
                portionen = 5;
            }
            List<string> fehlendeZutaten = new List<string>();
            for (int i = 0; i < r.Zutaten.Count; i++)
            {
                string zutat = r.Zutaten[i];
                double benoetigt = r.ZutatenMengen[i] * portionen;
                double vorhanden = PantryMengeVon(zutat);
                if (vorhanden < benoetigt)
                {
                    double fehlt = benoetigt - vorhanden;
                    fehlendeZutaten.Add(zutat + " (" + Math.Ceiling(fehlt) + "g)");
                }
            }
            if (fehlendeZutaten.Count > 0)
            {
                string fehltText = "";
                foreach (string f in fehlendeZutaten)
                {
                    fehltText += "• " + f + "\n";
                }
                MessageBox.Show("Fehlende Zutaten für " + portionen + " Portion(en):\n\n" + fehltText, "Achtung", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            for (int i = 0; i < r.Zutaten.Count; i++)
            {
                ZieheAusPantryAb(r.Zutaten[i], r.ZutatenMengen[i] * portionen);
            }
            PantryItem.SpeichereAlle(pantryItems);
            double gesamtKal = r.KalorienProPortion * portionen;
            MahlzeitEintrag neuerEintrag = new MahlzeitEintrag(r.Name + " (" + portionen + "x)", 1, gesamtKal, 0, 0, 0);
            MahlzeitEintrag.Speichere(neuerEintrag);
            MessageBox.Show(r.Name + " (" + portionen + " Portionen) wurde gekocht!\n\n" + gesamtKal.ToString("0") + " kcal wurden hinzugefügt.", "Gekocht!", MessageBoxButton.OK, MessageBoxImage.Information);
            AppLogger.Info($"Rezept {r.Name} ({portionen} Portionen) gekocht. {gesamtKal} kcal hinzugefügt.");
            foreach (Rezept rezept in alleRezepte)
            {
                rezept.SetzeMatchProzent(rezeptRechner.BerechneMatch(rezept, pantryItems));
            }
            FiltereUndZeigeRezepte();
            if (aktuellesRezept != null)
            {
                ZeigeRezeptDetails(aktuellesRezept);
            }
        }

        private void btnSchliessen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}