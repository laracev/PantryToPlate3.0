using System;
using System.IO;

namespace PantryToPlate
{
    public static class Logger
    {
        private static string logDatei = "data/app_log.txt";

        public static void Info(string nachricht)
        {
            SchreibeLog("INFO", nachricht);
        }

        public static void Warnung(string nachricht)
        {
            SchreibeLog("WARNUNG", nachricht);
        }

        public static void Fehler(string nachricht)
        {
            SchreibeLog("FEHLER", nachricht);
        }

        public static void Fehler(string nachricht, Exception ex)
        {
            SchreibeLog("FEHLER", nachricht + " - " + ex.Message);
        }

        private static void SchreibeLog(string level, string nachricht)
        {
            try
            {
                if (!Directory.Exists("data"))
                {
                    Directory.CreateDirectory("data");
                }

                string eintrag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + level + "] " + nachricht + "\n";
                File.AppendAllText(logDatei, eintrag);
            }
            catch
            {

            }
        }

        public static string LeseLog()
        {
            if (File.Exists(logDatei))
            {
                return File.ReadAllText(logDatei);
            }
            return "Keine Log-Einträge vorhanden.";
        }
    }
}