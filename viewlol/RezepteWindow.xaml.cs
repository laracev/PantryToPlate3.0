using PantryToPlate.Models;
using PTP;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PantryToPlate
{
    public partial class RezepteWindow : Window
    {
        private List<Rezept> alleRezepte = new List<Rezept>();
        private List<Rezept> gefilterteRezepte = new List<Rezept>();
        private List<PantryItem> pantryItems = new List<PantryItem>();
        private Dictionary<string, double> lebensmittelKalorien = new Dictionary<string, double>();

        private string pantryDatei = "data/pantry.csv";
        private string einkaufslisteDatei = "data/Einkaufsliste.csv";
        private string rezepteDatei = "data/rezepte.csv";
        private string lebensmittelDatei = "data/test_utf8.csv";

        private Rezept aktuellesRezept = null;
        private string aktuelleKategorie = "Alle";
        private string aktuellerSuchtext = "";

        public RezepteWindow()
        {
            InitializeComponent();
            this.Loaded += RezepteWindow_Loaded;
        }

        private void RezepteWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= RezepteWindow_Loaded;
            cboKategorie.SelectionChanged += cboKategorie_SelectionChanged;
            txtSuche.TextChanged += txtSuche_TextChanged;
            LadeAlleDaten();
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

        private string HoleKategorieAusName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Sonstige";
            }

            string nameLower = name.ToLower();
            string[] woerter = nameLower.Split(' ');

            if (woerter.Length > 0)
            {
                string erstesWort = woerter[0];

                if (erstesWort == "pasta") return "Pasta";
                if (erstesWort == "bowl") return "Bowl";
                if (erstesWort == "normale") return "Normale Pizza";
                if (erstesWort == "vegane") return "Vegane Pizza";
                if (erstesWort == "curry") return "Curry";
                if (erstesWort == "suppe") return "Suppe";
                if (erstesWort == "auflauf") return "Auflauf";
                if (erstesWort == "omelett") return "Omelett";
                if (erstesWort == "wrap") return "Wrap";
                if (erstesWort == "pfannengericht") return "Pfannengericht";
                if (erstesWort == "ofengericht") return "Ofengericht";
                if (erstesWort == "kuchen") return "Kuchen";
                if (erstesWort == "kekse") return "Kekse";
                if (erstesWort == "smoothie") return "Smoothie";
                if (erstesWort == "frühstück") return "Frühstück";
                if (erstesWort == "dessert") return "Dessert";
            }

            return "Sonstige";
        }

        private void LadeAlleDaten()
        {
            LadeLebensmittel();
            LadePantry();
            LadeRezepte();
            FiltereUndZeigeRezepte();
        }

        private void LadeLebensmittel()
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
                Logger.Fehler("Rezeptdatei konnte nicht geladen werden");
                return;
            }

            string[] zeilen = File.ReadAllLines(rezepteDatei);

            for (int i = 1; i < zeilen.Length; i++)
            {
                string zeile = zeilen[i];
                if (string.IsNullOrWhiteSpace(zeile))
                {
                    continue;
                }

                Rezept r = ParseRezept(zeile);
                if (r != null && r.Zutaten.Count > 0)
                {
                    if (r.KalorienProPortion <= 0)
                    {
                        BerechneRezeptKalorien(r);
                    }
                    BerechneMatchProzent(r);
                    alleRezepte.Add(r);
                }
            }
        }

        private Rezept ParseRezept(string zeile)
        {
            try
            {
                string[] teile = zeile.Split(';');
                if (teile.Length < 3) return null;

                Rezept r = new Rezept();
                r.Name = teile[0].Trim();
                if (string.IsNullOrWhiteSpace(r.Name)) return null;

                // Kalorien aus CSV holen (letzte Spalte)
                if (teile.Length >= 5)
                {
                    string kalText = teile[4].Trim().Replace(" kcal", "").Replace(",", ".");
                    double kalorienAusCSV = 0;
                    try
                    {
                        kalorienAusCSV = Convert.ToDouble(kalText, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        kalorienAusCSV = 0;
                    }

                    if (kalorienAusCSV > 0)
                    {
                        r.KalorienProPortion = kalorienAusCSV;
                    }
                }

                // Anleitung
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

                // Zutaten
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
                        double menge = ZahlLesen(mengeText);

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

        private void BerechneRezeptKalorien(Rezept r)
        {
            if (r.KalorienProPortion > 0)
            {
                return;
            }

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
                    foreach (string key in lebensmittelKalorien.Keys)
                    {
                        if (key.Contains(zutat) || zutat.Contains(key))
                        {
                            kalorienPro100g = lebensmittelKalorien[key];
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
                string zutatRezept = BereinigeZutatName(r.Zutaten[i]);
                double benoetigt = r.ZutatenMengen[i];
                bool gefunden = false;
                for (int j = 0; j < pantryItems.Count; j++)
                {
                    string pantryName = BereinigeZutatName(pantryItems[j].Name);
                    if (pantryName.ToLower() == zutatRezept.ToLower() && pantryItems[j].Menge >= benoetigt)
                    {
                        gefunden = true;
                        break;
                    }
                }
                if (gefunden) vorhanden++;
            }
            if (r.Zutaten.Count > 0)
                r.MatchProzent = (vorhanden * 100) / r.Zutaten.Count;
            else
                r.MatchProzent = 0;
        }
        private void FiltereUndZeigeRezepte()
        {
            gefilterteRezepte.Clear();

            for (int i = 0; i < alleRezepte.Count; i++)
            {
                Rezept r = alleRezepte[i];

                if (aktuelleKategorie != "Alle")
                {
                    string rezeptKategorie = HoleKategorieAusName(r.Name);
                    if (rezeptKategorie != aktuelleKategorie)
                    {
                        continue;
                    }
                }

                if (aktuellerSuchtext != "")
                {
                    if (!r.Name.ToLower().Contains(aktuellerSuchtext))
                    {
                        continue;
                    }
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

                string matchColorHex = "#9ABF75";

                string nameKurz = r.Name;
                if (nameKurz.Length > 55)
                {
                    nameKurz = nameKurz.Substring(0, 52) + "...";
                }

                Border border = new Border();
                border.Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
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
                matchTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
                sp.Children.Add(matchTextBlock);

                grid.Children.Add(sp);

                Border prozentBorder = new Border();
                prozentBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(matchColorHex));
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
                emptyText.Foreground = new SolidColorBrush(Colors.Gray);
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

        private void ZeigeRezeptDetails(Rezept r)
        {
            if (r == null)
            {
                return;
            }

            aktuellesRezept = r;

            txtKeinRezept.Visibility = Visibility.Collapsed;
            spDetailsInhalt.Visibility = Visibility.Visible;

            txtRezeptName.Text = r.Name;
            txtKalorien.Text = r.KalorienProPortion.ToString("0") + " kcal";
            txtAnleitung.Text = r.Anleitung;


            spZutaten.Children.Clear();
            List<string> fehlendeZutaten = new List<string>();

            for (int i = 0; i < r.Zutaten.Count; i++)
            {
                string zutatRoh = r.Zutaten[i];
                string zutatBereinigt = BereinigeZutatName(zutatRoh);
                double menge = r.ZutatenMengen[i];

                bool istVorhanden = false;
                for (int j = 0; j < pantryItems.Count; j++)
                {
                    string pantryNameBereinigt = BereinigeZutatName(pantryItems[j].Name);
                    if (pantryNameBereinigt.ToLower() == zutatBereinigt.ToLower() &&
                        pantryItems[j].Menge >= menge)
                    {
                        istVorhanden = true;
                        break;
                    }
                }

                Border zutatBorder = new Border();
                zutatBorder.Padding = new Thickness(8, 5, 8, 5);
                zutatBorder.Margin = new Thickness(0, 2, 0, 2);
                zutatBorder.CornerRadius = new CornerRadius(6);

                TextBlock zutatText = new TextBlock();
                zutatText.FontSize = 12;

                if (istVorhanden)
                {
                    zutatBorder.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
                    zutatText.Text = "✓ " + zutatRoh + " (" + menge + "g)";
                }
                else
                {
                    zutatBorder.Background = new SolidColorBrush(Color.FromRgb(253, 235, 236));
                    zutatText.Text = "✗ " + zutatRoh + " (" + menge + "g) - fehlt";
                    fehlendeZutaten.Add(zutatRoh);
                }

                zutatBorder.Child = zutatText;
                spZutaten.Children.Add(zutatBorder);
            }

            btnFehlendeKaufen.Tag = fehlendeZutaten;
            btnRezeptKochen.Tag = r;
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
            if (txtSuche.Text != null)
            {
                aktuellerSuchtext = txtSuche.Text.ToLower();
            }
            else
            {
                aktuellerSuchtext = "";
            }
            FiltereUndZeigeRezepte();
        }

        // Entfernt alles ab der ersten öffnenden Klammer und trimmt
        private string BereinigeZutatName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;
            int klammerIndex = name.IndexOf('(');
            if (klammerIndex > 0)
                name = name.Substring(0, klammerIndex);
            return name.Trim();
        }

        private string NameOhneMenge(string text)
        {
            if (text.Contains("("))
            {
                return text.Substring(0, text.IndexOf('(')).Trim();
            }
            return text.Trim();
        }

        private double MengeAusText(string text)
        {
            // Beispiel: "Kürbis Hokkaido (100g)"  ->  100
            int start = text.IndexOf('(');
            int ende = text.IndexOf('g');
            if (start >= 0 && ende > start)
            {
                string zahlText = text.Substring(start + 1, ende - start - 1);
                return ZahlLesen(zahlText);
            }
            return 0; // keine Klammer mit g gefunden
        }

        private double PantryMengeVon(string zutat)
        {
            string zutatBereinigt = BereinigeZutatName(zutat).ToLower();
            for (int i = 0; i < pantryItems.Count; i++)
            {
                string pantryNameBereinigt = BereinigeZutatName(pantryItems[i].Name).ToLower();
                if (pantryNameBereinigt == zutatBereinigt)
                {
                    return pantryItems[i].Menge;
                }
            }
            return 0;
        }

        // Fehlende Zutaten zur Einkaufsliste hinzufügen (korrigiert)
        private void btnFehlendeKaufen_Click(object sender, RoutedEventArgs e)
        {
            if (aktuellesRezept == null)
            {
                MessageBox.Show("Bitte zuerst ein Rezept auswählen.");
                return;
            }

            // Bestehende Einkaufsliste einlesen
            List<string> einkaufsListe = new List<string>();
            if (File.Exists(einkaufslisteDatei))
            {
                string[] zeilen = File.ReadAllLines(einkaufslisteDatei);
                for (int i = 1; i < zeilen.Length; i++) // erste Zeile ist Header
                {
                    if (!string.IsNullOrWhiteSpace(zeilen[i]))
                        einkaufsListe.Add(zeilen[i].Trim());
                }
            }

            List<string> hinzugefuegt = new List<string>();

            for (int i = 0; i < aktuellesRezept.Zutaten.Count; i++)
            {
                string zutat = aktuellesRezept.Zutaten[i];
                double benoetigt = aktuellesRezept.ZutatenMengen[i];
                double vorhanden = PantryMengeVon(zutat);
                double fehlt = benoetigt - vorhanden;
                if (fehlt <= 0) continue;

                int fehltGerundet = (int)Math.Ceiling(fehlt);
                string neuerEintrag = zutat + " (" + fehltGerundet + "g)";
                bool schonVorhanden = false;

                // Prüfen, ob diese Zutat schon auf der Liste steht (gleicher Name ohne Menge)
                for (int j = 0; j < einkaufsListe.Count; j++)
                {
                    string vorhandenerName = NameOhneMenge(einkaufsListe[j]);
                    string vorhandenBereinigt = BereinigeZutatName(vorhandenerName);
                    string zutatBereinigt = BereinigeZutatName(zutat);
                    if (vorhandenBereinigt.ToLower() == zutatBereinigt.ToLower())
                    {
                        // Überschreibe mit dem neuen Eintrag (aktuelle Menge)
                        einkaufsListe[j] = neuerEintrag;
                        schonVorhanden = true;
                        break;
                    }
                }
                if (!schonVorhanden)
                {
                    einkaufsListe.Add(neuerEintrag);
                }
                hinzugefuegt.Add(neuerEintrag);
            }

            if (hinzugefuegt.Count == 0)
            {
                MessageBox.Show("Keine fehlenden Zutaten – alles ist bereits in der Pantry oder auf der Liste.");
                return;
            }

            // Speichern
            string inhalt = "Zutat\n";
            for (int i = 0; i < einkaufsListe.Count; i++)
            {
                inhalt += einkaufsListe[i] + "\n";
            }
            Directory.CreateDirectory("data");
            File.WriteAllText(einkaufslisteDatei, inhalt);

            string msg = "Zur Einkaufsliste hinzugefügt:\n";
            for (int i = 0; i < hinzugefuegt.Count; i++)
            {
                msg += "• " + hinzugefuegt[i] + "\n";
            }
            MessageBox.Show(msg, "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void btnRezeptKochen_Click(object sender, RoutedEventArgs e)
        {
            Rezept r = (Rezept)btnRezeptKochen.Tag;
            if (r == null) return;

            string portionenText = Microsoft.VisualBasic.Interaction.InputBox(
                "Wie viele Portionen möchtest du kochen?\n(1-5 Portionen möglich)",
                "Portionen auswählen",
                "1");
            int portionen = 1;
            try { portionen = Convert.ToInt32(portionenText); }
            catch { portionen = 1; }
            if (portionen < 1) portionen = 1;
            if (portionen > 5) portionen = 5;

            // Prüfen, ob alle Zutaten in ausreichender Menge vorhanden sind
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
                for (int i = 0; i < fehlendeZutaten.Count; i++)
                {
                    fehltText += "• " + fehlendeZutaten[i] + "\n";
                }
                MessageBox.Show("Fehlende Zutaten für " + portionen + " Portion(en):\n\n" + fehltText,
                                "Achtung", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Zutaten aus der Pantry abziehen (exakte Namensübereinstimmung)
            for (int i = 0; i < r.Zutaten.Count; i++)
            {
                string zutatRoh = r.Zutaten[i];
                string zutatBereinigt = BereinigeZutatName(zutatRoh);
                double benoetigt = r.ZutatenMengen[i] * portionen;
                for (int j = 0; j < pantryItems.Count; j++)
                {
                    string pantryNameBereinigt = BereinigeZutatName(pantryItems[j].Name);
                    if (pantryNameBereinigt.ToLower() == zutatBereinigt.ToLower())
                    {
                        pantryItems[j].Menge -= benoetigt;
                        if (pantryItems[j].Menge < 0) pantryItems[j].Menge = 0;
                        break;
                    }
                }
            }

            // Pantry speichern
            string pantryInhalt = "Name;Menge\n";
            for (int i = 0; i < pantryItems.Count; i++)
            {
                if (pantryItems[i].Menge > 0)
                {
                    pantryInhalt += pantryItems[i].Name + ";" + pantryItems[i].Menge + "\n";
                }
            }
            File.WriteAllText(pantryDatei, pantryInhalt);

            // Mahlzeit speichern
            string heute = DateTime.Now.ToString("yyyy-MM-dd");
            string mahlzeitenDatei = "data/tagesmahlzeiten.csv";
            double gesamtKal = r.KalorienProPortion * portionen;
            string zeile = heute + ";" + r.Name + " (" + portionen + "x);1;" + gesamtKal + ";0;0;0";

            if (File.Exists(mahlzeitenDatei))
            {
                File.AppendAllText(mahlzeitenDatei, "\n" + zeile);
            }
            else
            {
                File.WriteAllText(mahlzeitenDatei, "Datum;Lebensmittel;Gramm;Kalorien;Proteine;Kohlenhydrate;Fett\n" + zeile);
            }

            MessageBox.Show(r.Name + " (" + portionen + " Portionen) wurde gekocht!\n\n" +
                            gesamtKal.ToString("0") + " kcal wurden hinzugefügt.",
                            "Gekocht!", MessageBoxButton.OK, MessageBoxImage.Information);

            // Pantry neu laden und Match-Prozente aktualisieren
            LadePantry();
            for (int i = 0; i < alleRezepte.Count; i++)
            {
                BerechneMatchProzent(alleRezepte[i]);
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