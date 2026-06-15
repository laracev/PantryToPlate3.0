using PantryToPlate;
using System.Windows;

namespace PTP
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppLogger.Init();
            AppLogger.Info("Anwendung gestartet");
            SplashScreenWindow splash = new SplashScreenWindow();
            splash.Show();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppLogger.CloseAndFlush();
            base.OnExit(e);
        }
    }
}