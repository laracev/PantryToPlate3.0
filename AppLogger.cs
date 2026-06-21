using Serilog;
using System;
using System.IO;

namespace PantryToPlate
{
    public static class AppLogger
    {
        private static bool istInitialisiert = false;

        public static void Init()
        {
            if (istInitialisiert)
            {
                return;
            }

            Directory.CreateDirectory("logs");

            Log.Logger = new LoggerConfiguration().WriteTo.File(Path.Combine("logs", "log-.txt"), rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}").CreateLogger();

            istInitialisiert = true;
        }

        public static void Info(string message)
        {
            if (!istInitialisiert) Init();
            Log.Information(message);
        }

        public static void Error(string message)
        {
            if (!istInitialisiert) Init();
            Log.Error(message);
        }

        public static void Error(Exception ex, string message)
        {
            if (!istInitialisiert) Init();
            Log.Error(ex, message);
        }

        public static void CloseAndFlush()
        {
            if (istInitialisiert)
            {
                Log.CloseAndFlush();
            }
        }
    }
}