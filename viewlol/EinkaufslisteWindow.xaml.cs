using PantryToPlate.helpers;
using PantryToPlate.Models;
using System;
using System.Collections.Generic;
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
        private List<PantryItem> pantryItems = new List<PantryItem>();

        public EinkaufslisteWindow()
        {
            InitializeComponent();
            LadePantry();
            LadeEinkaufsliste();
        }

        private void LadePantry()
        {
            try
            {
                pantryItems = PantryItem.LadeAlleAusCsv();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Laden der Pantry");
                pantryItems = new List<PantryItem>();
            }
        }

        private void LadeEinkaufsliste()
        {
            try
            {
                List<string> eintraege = Einkaufsliste.Lade();
                einkaufsliste.Clear();
                foreach (string e in eintraege)
                {
                    einkaufsliste.Add(new EinkaufsItemMitCheckbox() { Name = e, IstGekauft = false });
                }
                ZeigeEinkaufsliste();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Laden der Einkaufsliste");
                MessageBox.Show("Fehler beim Laden der Einkaufsliste.");
            }
        }


        //chatgpt: promt: kannst du mir helfen das es halt so schöner angezeigt wird und bissl besser so yk
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
                Grid.SetColumn(chk, 0);

                TextBlock txt = new TextBlock();
                txt.Text = item.Name;
                txt.FontSize = 14;
                if (item.IstGekauft)
                {
                    txt.TextDecorations = TextDecorations.Strikethrough;
                    txt.Foreground = new SolidColorBrush(Colors.Gray);
                }
                else
                {
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
        //chatgpt ende

        private void CheckboxGeklickt(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            int index = (int)chk.Tag;
            if (index >= 0 && index < einkaufsliste.Count)
            {
                einkaufsliste[index].IstGekauft = (chk.IsChecked == true);
                ZeigeEinkaufsliste();
            }
        }

        private void SpeichereEinkaufsliste()
        {
            List<string> eintraege = new List<string>();
            foreach (EinkaufsItemMitCheckbox item in einkaufsliste)
            {
                if (!item.IstGekauft)
                {
                    eintraege.Add(item.Name);
                }
            }
            Einkaufsliste.Speichere(eintraege);
        }


        // chatgpt, promt: kannst du mir helfen das es einfach den besten namen sucht anstatt den genauen namen so checkst du
        private string FindeBestenLebensmittelNamen(string itemName)
        {
            List<Lebensmittel> alleLebensmittel = Lebensmittel.LadeAlleAusCsv();
            string besterName = itemName;
            int besterScore = 0;
            foreach (Lebensmittel lm in alleLebensmittel)
            {
                int score = Namensvergleich.BerechneAehnlichkeit(itemName, lm.Name);
                if (score > besterScore)
                {
                    besterScore = score;
                    besterName = lm.Name;
                }
            }
            return (besterScore >= 120) ? besterName : itemName;
        }
        //chatgpt ende
        private void btnGekaufteEntfernen_Click(object sender, RoutedEventArgs e)
        {
            List<string> gekaufteItems = new List<string>();
            List<double> gekaufteMengen = new List<double>();
            List<EinkaufsItemMitCheckbox> neueListe = new List<EinkaufsItemMitCheckbox>();

            foreach (EinkaufsItemMitCheckbox item in einkaufsliste)
            {
                if (item.IstGekauft)
                {
             
                    gekaufteMengen.Add(Namensvergleich.MengeAusText(item.Name));
                }
                else
                {
                    neueListe.Add(item);
                }
            }

            if (gekaufteItems.Count == 0)
            {
                MessageBox.Show("Keine Items zum Einkaufen ausgewählt!\nBitte hake ab, was du gekauft hast.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            for (int i = 0; i < gekaufteItems.Count; i++)
            {
                string passenderName = FindeBestenLebensmittelNamen(gekaufteItems[i]);
                double menge = gekaufteMengen[i];
                bool gefunden = false;
                foreach (PantryItem p in pantryItems)
                {
                    if (Namensvergleich.NormalisiereName(p.Name) == Namensvergleich.NormalisiereName(passenderName))
                    {
                        p.Menge += menge;
                        gefunden = true;
                        break;
                    }
                }
                if (!gefunden)
                {
                    pantryItems.Add(new PantryItem(passenderName, menge));
                }
                gekaufteItems[i] = passenderName;
            }

            try
            {
                PantryItem.SpeichereAlle(pantryItems);
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Fehler beim Speichern der Pantry");
                MessageBox.Show("Fehler beim Speichern der Pantry.");
                return;
            }

            einkaufsliste = neueListe;
            SpeichereEinkaufsliste();
            ZeigeEinkaufsliste();

            string gekaufteListe = "";
            for (int i = 0; i < gekaufteItems.Count; i++)
            {
                if (gekaufteListe != "")
                {
                    gekaufteListe += "\n";
                }
                gekaufteListe += "✓ " + gekaufteItems[i] + " (" + gekaufteMengen[i].ToString("0") + "g)";
            }
            MessageBox.Show("Folgende Items wurden zur Pantry hinzugefügt:\n\n" + gekaufteListe, "Erfolg!", MessageBoxButton.OK, MessageBoxImage.Information);
            AppLogger.Info("Gekaufte Items zur Pantry hinzugefügt: " + gekaufteListe);
        }

        private void btnLeeren_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Wirklich die gesamte Einkaufsliste leeren?", "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                einkaufsliste.Clear();
                SpeichereEinkaufsliste();
                ZeigeEinkaufsliste();
                AppLogger.Info("Einkaufsliste geleert");
            }
        }

        private void btnSchliessen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}