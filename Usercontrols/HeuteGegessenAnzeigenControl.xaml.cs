using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PantryToPlate.Usercontrols
{
    public partial class HeuteGegessenAnzeigenControl : UserControl
    {
        public event Action<int> MahlzeitLoeschenAngefordert;

        public HeuteGegessenAnzeigenControl()
        {
            InitializeComponent();
            LeereAnzeige();
        }

        public void SetzeMahlzeiten(List<string> namen, List<double> kalorien)
        {
            LeereAnzeige();

            Label[] mahlzeitLabels =
            {
                ersteMahlzeitLabel,
                zweiteMahlzeitLabel,
                dritteMahlzeitLabel,
                vierteMahlzeitLabel
            };

            Label[] kalorienLabels =
            {
                ersteMahlzeitKalorienLabel,
                zweiteMahlzeitKalorienLabel,
                dritteMahlzeitKalorienLabel,
                vierteMahlzeitKalorienLabel
            };

            Button[] loeschButtons =
            {
                btnMahlzeit1Loeschen,
                btnMahlzeit2Loeschen,
                btnMahlzeit3Loeschen,
                btnMahlzeit4Loeschen
            };

            int anzahl = namen.Count;

            if (anzahl > 4)
            {
                anzahl = 4;
            }

            for (int i = 0; i < anzahl; i++)
            {
                mahlzeitLabels[i].Content = namen[i];
                kalorienLabels[i].Content = kalorien[i].ToString("0") + " kcal";
                loeschButtons[i].Visibility = Visibility.Visible;
            }
        }

        private void btnMahlzeitLoeschen_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            if (!int.TryParse(button.Tag.ToString(), out int index))
            {
                return;
            }

            if (MahlzeitLoeschenAngefordert != null)
            {
                MahlzeitLoeschenAngefordert(index);
            }
        }

        private void LeereAnzeige()
        {
            ersteMahlzeitLabel.Content = "-";
            zweiteMahlzeitLabel.Content = "-";
            dritteMahlzeitLabel.Content = "-";
            vierteMahlzeitLabel.Content = "-";

            ersteMahlzeitKalorienLabel.Content = "";
            zweiteMahlzeitKalorienLabel.Content = "";
            dritteMahlzeitKalorienLabel.Content = "";
            vierteMahlzeitKalorienLabel.Content = "";

            btnMahlzeit1Loeschen.Visibility = Visibility.Collapsed; //durch viel ausprobieren hinbekommen
            btnMahlzeit2Loeschen.Visibility = Visibility.Collapsed;
            btnMahlzeit3Loeschen.Visibility = Visibility.Collapsed;
            btnMahlzeit4Loeschen.Visibility = Visibility.Collapsed;
        }
    }
}
