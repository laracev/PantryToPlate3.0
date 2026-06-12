using System.Collections.Generic;
using System.Windows.Controls;

namespace PantryToPlate.Usercontrols
{
    public partial class HeuteGegessenAnzeigenControl : UserControl
    {
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

            int anzahl = namen.Count;

            if (anzahl > 4)
            {
                anzahl = 4;
            }

            for (int i = 0; i < anzahl; i++)
            {
                mahlzeitLabels[i].Content = namen[i];
                kalorienLabels[i].Content = kalorien[i].ToString("0") + " kcal";
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
        }
    }
}