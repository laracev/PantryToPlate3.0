using System;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
namespace PantryToPlate
{
    public static class ThemeManager
    {
        private const string EinstellungsDatei = "data/theme.txt";

        public static bool IsDarkMode { get; private set; }


        //chatgpt start
        public static void Initialize()
        {
            bool darkMode = false;

            try
            {
                if (File.Exists(EinstellungsDatei))
                {
                    darkMode = File.ReadAllText(EinstellungsDatei).Trim().Equals("dark", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                darkMode = false;
            }

            SetDarkMode(darkMode, false);
        }
        //chatgpt end

        public static void SetDarkMode(bool darkMode)
        {
            SetDarkMode(darkMode, true);
        }

        private static void SetDarkMode(bool darkMode, bool speichern)
        {
            Collection<ResourceDictionary> dictionaries = Application.Current.Resources.MergedDictionaries;

            for (int i = dictionaries.Count - 1; i >= 0; i--)
            {
                string source = dictionaries[i].Source?.OriginalString ?? string.Empty;

                if (source.EndsWith("Theme.Light.xaml", StringComparison.OrdinalIgnoreCase) || source.EndsWith("Theme.Dark.xaml", StringComparison.OrdinalIgnoreCase))
                {
                    dictionaries.RemoveAt(i);
                }
            }

            string datei = darkMode ? "Themes/Theme.Dark.xaml" : "Themes/Theme.Light.xaml";
            dictionaries.Insert(0, new ResourceDictionary
            {
                Source = new Uri(datei, UriKind.Relative)
            });

            IsDarkMode = darkMode;

            if (!speichern)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory("data");
                File.WriteAllText(EinstellungsDatei, darkMode ? "dark" : "light");
            }
            catch
            {
                AppLogger.Error("es konnte nicht gespeichert werden...");
            }
        }
    }
}
