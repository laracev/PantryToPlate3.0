using PantryToPlate.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using PantryToPlate.Models;
using PTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
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
            File.WriteAllText(einkaufslisteDatei, inhalt);
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
            int start = text.IndexOf('(');
            int ende = text.IndexOf('g');
            if (start >= 0 && ende > start)
            {
                string zahlText = text.Substring(start + 1, ende - start - 1);
                return ZahlLesen(zahlText);
            }
            return 0;
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
                double menge = gekaufteMengen[i];
                bool gefunden = false;

                for (int j = 0; j < pantryItems.Count; j++)
                {
                    if (pantryItems[j].Name.ToLower() == itemName.ToLower())
                    {
                        pantryItems[j].Menge = pantryItems[j].Menge + menge;
                        gefunden = true;
                        break;
                    }
                }

                if (!gefunden)
                {
                    PantryItem neuesItem = new PantryItem();
                    neuesItem.Name = itemName;
                    neuesItem.Menge = menge;
                    pantryItems.Add(neuesItem);
                }
            }

            string pantryInhalt = "Name;Menge\n";
            for (int i = 0; i < pantryItems.Count; i++)
            {
                if (pantryItems[i].Menge > 0)
                {
                    pantryInhalt = pantryInhalt + pantryItems[i].Name + ";" + pantryItems[i].Menge + "\n";
                }
            }
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