using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PantryToPlate.Usercontrols
{
    /// <summary>
    /// Interaktionslogik für KalorienFortschrittControl.xaml
    /// </summary>
    public partial class KalorienFortschrittControl : UserControl
    {
        private double aktuellerWert = 0;
        private double maximalWert = 2000;

        public KalorienFortschrittControl()
        {
            InitializeComponent();
            ZeichneFortschritt(0);
        }
        public void SetzeFortschritt(double aktuell, double maximal)
        {
            aktuellerWert = aktuell;
            maximalWert = maximal;

            double prozent = 0;
            if (maximal > 0)
            {
                prozent = (aktuell / maximal) * 100;
                if (prozent > 100)
                {
                    prozent = 100;
                }
            }

            txtProzent.Text = ((int)prozent).ToString() + "%";

            if (prozent > 90)
            {
                fortschrittsBogen.Stroke = new SolidColorBrush(Colors.IndianRed);
            }
            else if (prozent > 70)
            {
                fortschrittsBogen.Stroke = new SolidColorBrush(Colors.LightGoldenrodYellow);
            }
            else
            {
                fortschrittsBogen.Stroke = new SolidColorBrush(Colors.LawnGreen);
            }

            ZeichneFortschritt(prozent);
        }


        //Mit bissl chatgpt hilfe gemacht, also immer wieder mal paar ki hilfen weil kann kein mathe lul
        private void ZeichneFortschritt(double prozent)
        {
            double winkel = (prozent / 100.0) * 360.0;

            double startWinkel = -90;
            double endWinkel = startWinkel + winkel;

            double startRad = startWinkel * Math.PI / 180.0;
            double endRad = endWinkel * Math.PI / 180.0;

            double radius = 80;
            double centerX = 90;
            double centerY = 90;

            double startX = centerX + radius * Math.Cos(startRad);
            double startY = centerY + radius * Math.Sin(startRad);

            double endX = centerX + radius * Math.Cos(endRad);
            double endY = centerY + radius * Math.Sin(endRad);

            bool isLargeArc = winkel > 180;


            //das hier ist nur ki
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(startX, startY), false, false);
                ctx.ArcTo(new Point(endX, endY), new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true, false);
            }

            fortschrittsBogen.Data = geometry;

            //nur KI ende
        }
        //ki ende


    }
}
