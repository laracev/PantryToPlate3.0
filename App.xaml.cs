using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;

namespace PTP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static CultureInfo DeutscheKultur = new CultureInfo("de-DE");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            StelleSicherDassDatenExistieren();
        }

        private void StelleSicherDassDatenExistieren()
        {
            Directory.CreateDirectory("data");

            string einkaufslisteDatei = "data/Einkaufsliste.csv";
            if (!File.Exists(einkaufslisteDatei))
            {
                File.WriteAllText(einkaufslisteDatei, "Zutat\n");
            }

            string pantryDatei = "data/pantry.csv";
            if (!File.Exists(pantryDatei))
            {
                File.WriteAllText(pantryDatei, "Name;Menge\n");
            }

            string tagesmahlzeitenDatei = "data/tagesmahlzeiten.csv";
            if (!File.Exists(tagesmahlzeitenDatei))
            {
                File.WriteAllText(tagesmahlzeitenDatei, "Datum;Lebensmittel;Gramm;Kalorien;Proteine;Kohlenhydrate;Fett\n");
            }

            string benutzerDatei = "data/benutzer.csv";
            if (!File.Exists(benutzerDatei))
            {
                File.WriteAllText(benutzerDatei, "Kalorienziel;Gewicht;Groesse;Alter;Geschlecht;Aktivitaet;Ziel\n");
            }

            string fitnessDatei = "data/FitnessEintraege.csv";
            if (!File.Exists(fitnessDatei))
            {
                File.WriteAllText(fitnessDatei, "Datum;Aktivitaet;Kalorien\n");
            }
        }

    }
}