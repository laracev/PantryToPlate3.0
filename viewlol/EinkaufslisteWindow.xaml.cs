using PantryToPlate.Models;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PantryToPlate
{
    public partial class EinkaufslisteWindow : Window
    {
        public class EinkaufsItemMitCheckbox
        {
            public string Name { get; set; }
            public bool IstGekauft { get; set; }
        }

        private List<EinkaufsItemMitCheckbox> einkaufsliste = new List<EinkaufsItemMitCheckbox>();
        private string einkaufslisteDatei = "data/Einkaufsliste.csv";
        private string pantryDatei = "data/pantry.csv";
        private List<PantryItem> pantryItems = new List<PantryItem>();

        public EinkaufslisteWindow()
        {
            InitializeComponent();
            LadePantry();
            LadeEinkaufsliste();
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
                        PantryItem item = new PantryItem();
                        item.Name = teile[0];
                        item.Menge = ZahlLesen(teile[1]);
                        pantryItems.Add(item);
                    }
                }
            }
        }

        private void LadeEinkaufsliste()
        {
            einkaufsliste.Clear();

            if (File.Exists(einkaufslisteDatei))
            {
                string[] zeilen = File.ReadAllLines(einkaufslisteDatei);
                for (int i = 1; i < zeilen.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(zeilen[i]))
                    {
                        EinkaufsItemMitCheckbox item = new EinkaufsItemMitCheckbox();
                        item.Name = zeilen[i].Trim();
                        item.IstGekauft = false;
                        einkaufsliste.Add(item);
                    }
                }
            }

            ZeigeEinkaufsliste();
        }

        private void ZeigeEinkaufsliste()
        {
            spEinkaufsliste.Children.Clear();

            for (int i = 0; i < einkaufsliste.Count; i++)
            {
                EinkaufsItemMitCheckbox item = einkaufsliste[i];

                Border itemBorder = new Border();
                itemBorder.Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
                itemBorder.CornerRadius = new CornerRadius(8);
                itemBorder.Padding = new Thickness(12, 8, 12, 8);
                itemBorder.Margin = new Thickness(0, 5, 0, 5);

                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                CheckBox chk = new CheckBox();
                chk.IsChecked = item.IstGekauft;
                chk.Tag = i;
                chk.Click += CheckboxGeklickt;
                chk.VerticalAlignment = VerticalAlignment.Center;
                chk.Margin = new Thickness(0, 0, 10, 0);
                Grid.SetColumn(chk, 0);

                TextBlock txt = new TextBlock();
                txt.Text = item.Name;
                txt.FontSize = 14;
                txt.VerticalAlignment = VerticalAlignment.Center;

                if (item.IstGekauft)
                {
                    txt.TextDecorations = TextDecorations.Strikethrough;
                    txt.Foreground = new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    txt.TextDecorations = null;
                    txt.Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59));
                }

                Grid.SetColumn(txt, 1);

                grid.Children.Add(chk);
                grid.Children.Add(txt);
                itemBorder.Child = grid;
                spEinkaufsliste.Children.Add(itemBorder);
            }

            if (einkaufsliste.Count == 0)
            {
                TextBlock emptyText = new TextBlock();
                emptyText.Text = "✨ Die Einkaufsliste ist leer! ✨";
                emptyText.FontSize = 14;
                emptyText.Foreground = new SolidColorBrush(Colors.Gray);
                emptyText.HorizontalAlignment = HorizontalAlignment.Center;
                emptyText.Margin = new Thickness(0, 50, 0, 0);
                spEinkaufsliste.Children.Add(emptyText);
            }
        }

        private void CheckboxGeklickt(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            int index = (int)chk.Tag;

            if (index >= 0 && index < einkaufsliste.Count)
            {
                einkaufsliste[index].IstGekauft = chk.IsChecked == true;
                ZeigeEinkaufsliste();
            }
        }

        private void SpeichereEinkaufsliste()
        {
            string inhalt = "Zutat\n";
            for (int i = 0; i < einkaufsliste.Count; i++)
            {
                if (!einkaufsliste[i].IstGekauft)
                {
                    inhalt = inhalt + einkaufsliste[i].Name + "\n";
                }
            }
            Directory.CreateDirectory("data");
            File.WriteAllText(einkaufslisteDatei, inhalt);
        }


        private string NameOhneMenge(string text)
        {
            int letzteKlammer = text.LastIndexOf('(');
            if (letzteKlammer > 0)
            {
                return text.Substring(0, letzteKlammer).Trim();
            }
            return text.Trim();
        }
        private double MengeAusText(string text)
        {
            int start = text.LastIndexOf('(');
            int ende = text.IndexOf(')', start + 1);
            if (start >= 0 && ende > start)
            {
                string mengenText = text.Substring(start + 1, ende - start - 1).ToLower().Trim();
                double zahl = ZahlLesen(mengenText);

                if (mengenText.Contains("kg")) zahl = zahl * 1000;
                else if (mengenText.Contains("l") && !mengenText.Contains("ml")) zahl = zahl * 1000;

                if (zahl > 0) return zahl;
            }

            // Fallback, damit gekaufte Zutaten ohne Mengenangabe nicht verschwinden.
            return 100;
        }


        private string NormalisiereName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";

            name = NameOhneMenge(name).ToLower().Trim();
            name = name.Replace("ä", "ae");
            name = name.Replace("ö", "oe");
            name = name.Replace("ü", "ue");
            name = name.Replace("ß", "ss");
            name = name.Replace(",", " ");
            name = name.Replace("/", " ");
            name = name.Replace("-", " ");

            string[] woerter = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string ergebnis = "";

            for (int i = 0; i < woerter.Length; i++)
            {
                string wort = woerter[i].Trim();

                if (wort == "roh" || wort == "gekocht" || wort == "geschält" || wort == "geschaelt" ||
                    wort == "frisch" || wort == "getrocknet" || wort == "tiefgekuehlt" || wort == "tk" ||
                    wort == "klein" || wort == "gross" || wort == "groß" || wort == "gehackt" ||
                    wort == "gewuerfelt" || wort == "gewürfelt" || wort == "gerieben")
                {
                    continue;
                }

                // einfache Plural-Annäherung: kartoffeln -> kartoffel, tomaten -> tomate
                if (wort.Length > 5 && wort.EndsWith("n")) wort = wort.Substring(0, wort.Length - 1);
                if (wort.Length > 5 && wort.EndsWith("en")) wort = wort.Substring(0, wort.Length - 2);
                if (wort.Length > 5 && wort.EndsWith("e")) wort = wort.Substring(0, wort.Length - 1);
                if (wort.Length > 5 && wort.EndsWith("s")) wort = wort.Substring(0, wort.Length - 1);

                if (ergebnis != "") ergebnis = ergebnis + " ";
                ergebnis = ergebnis + wort;
            }

            return ergebnis.Trim();
        }
        private bool IstSinnvollesWort(string wort)
        {
            if (wort.Length <= 2) return false;

            bool nurZiffern = true;
            for (int i = 0; i < wort.Length; i++)
            {
                char c = wort[i];
                if (!char.IsDigit(c) && c != '.' && c != ',')
                {
                    nurZiffern = false;
                    break;
                }
            }
            return !nurZiffern;
        }
        private int BerechneAehnlichkeit(string gesuchterName, string lebensmittelName)
        {
            string gesucht = NormalisiereName(gesuchterName);
            string lebensmittel = NormalisiereName(lebensmittelName);

            if (gesucht == "" || lebensmittel == "") return 0;
            if (gesucht == lebensmittel) return 1000;
            if (gesucht.Contains(lebensmittel) || lebensmittel.Contains(gesucht)) return 800;

            string[] gesuchtWoerter = gesucht.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] lebensmittelWoerter = lebensmittel.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int score = 0;
            for (int i = 0; i < gesuchtWoerter.Length; i++)
            {
                string wortA = gesuchtWoerter[i];
                if (!IstSinnvollesWort(wortA)) continue;

                for (int j = 0; j < lebensmittelWoerter.Length; j++)
                {
                    string wortB = lebensmittelWoerter[j];
                    if (!IstSinnvollesWort(wortB)) continue;

                    if (wortA == wortB) score = score + 200;
                    else if (wortA.Contains(wortB) || wortB.Contains(wortA)) score = score + 120;
                }
            }

            // Fallback: erste paar Buchstaben gleich -> "wahrscheinlich dasselbe Lebensmittel"
            if (score == 0)
            {
                int praefixLaenge = 4;
                if (gesucht.Length >= praefixLaenge && lebensmittel.Length >= praefixLaenge)
                {
                    string praefixA = gesucht.Substring(0, praefixLaenge);
                    string praefixB = lebensmittel.Substring(0, praefixLaenge);
                    if (praefixA == praefixB)
                    {
                        score = 150;
                    }
                }
            }

            return score;
        }



        private string FindeBestenLebensmittelNamen(string itemName)
        {
            string besterName = itemName;
            int besterScore = 0;

            for (int i = 0; i < AppDaten.Lebensmittel.Count; i++)
            {
                string lebensmittelName = AppDaten.Lebensmittel[i].Name;
                int score = BerechneAehnlichkeit(itemName, lebensmittelName);

                if (score > besterScore)
                {
                    besterScore = score;
                    besterName = lebensmittelName;
                }
            }

            // Nur automatisch ersetzen, wenn die Ähnlichkeit sinnvoll ist.
            // Sonst wird der ursprüngliche Name verwendet.
            if (besterScore >= 120)
            {
                return besterName;
            }

            return itemName;
        }
        private void btnGekaufteEntfernen_Click(object sender, RoutedEventArgs e)
        {
            List<string> gekaufteItems = new List<string>();
            List<double> gekaufteMengen = new List<double>();
            List<EinkaufsItemMitCheckbox> neueListe = new List<EinkaufsItemMitCheckbox>();

            for (int i = 0; i < einkaufsliste.Count; i++)
            {
                if (einkaufsliste[i].IstGekauft)
                {
                    string name = einkaufsliste[i].Name;
                    gekaufteItems.Add(NameOhneMenge(name));
                    gekaufteMengen.Add(MengeAusText(name));
                }
                else
                {
                    neueListe.Add(einkaufsliste[i]);
                }
            }

            if (gekaufteItems.Count == 0)
            {
                MessageBox.Show("Keine Items zum Einkaufen ausgewählt!\nBitte hake ab was du gekauft hast.",
                               "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            for (int i = 0; i < gekaufteItems.Count; i++)
            {
                string itemName = gekaufteItems[i];
                string passenderName = FindeBestenLebensmittelNamen(itemName);
                double menge = gekaufteMengen[i];
                bool gefunden = false;

                for (int j = 0; j < pantryItems.Count; j++)
                {
                    if (NormalisiereName(pantryItems[j].Name) == NormalisiereName(passenderName))
                    {
                        pantryItems[j].Menge = pantryItems[j].Menge + menge;
                        gefunden = true;
                        break;
                    }
                }

                if (!gefunden)
                {
                    PantryItem neuesItem = new PantryItem();
                    neuesItem.Name = passenderName;
                    neuesItem.Menge = menge;
                    pantryItems.Add(neuesItem);
                }

                gekaufteItems[i] = passenderName;
            }

            string pantryInhalt = "Name;Menge\n";
            for (int i = 0; i < pantryItems.Count; i++)
            {
                if (pantryItems[i].Menge > 0)
                {
                    pantryInhalt = pantryInhalt + pantryItems[i].Name + ";" + pantryItems[i].Menge.ToString(CultureInfo.InvariantCulture) + "\n";
                }
            }
            Directory.CreateDirectory("data");
            File.WriteAllText(pantryDatei, pantryInhalt);

            einkaufsliste = neueListe;
            SpeichereEinkaufsliste();

            string gekaufteListe = "";
            for (int i = 0; i < gekaufteItems.Count; i++)
            {
                if (gekaufteListe != "")
                {
                    gekaufteListe = gekaufteListe + "\n";
                }
                gekaufteListe = gekaufteListe + "✓ " + gekaufteItems[i] + " (" + gekaufteMengen[i].ToString("0") + "g)";
            }

            MessageBox.Show("Folgende Items wurden zur Pantry hinzugefügt:\n\n" + gekaufteListe,
                           "Erfolg!", MessageBoxButton.OK, MessageBoxImage.Information);

            ZeigeEinkaufsliste();
        }

        private void btnLeeren_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Wirklich die gesamte Einkaufsliste leeren?", "Bestätigen",
                               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                einkaufsliste.Clear();
                SpeichereEinkaufsliste();
                ZeigeEinkaufsliste();
            }
        }

        private void btnSchliessen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}