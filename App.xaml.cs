using System;
using System.IO;
using System.Windows;

namespace CountdownWidget
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            base.OnStartup(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = (Exception)e.ExceptionObject;
                File.WriteAllText("crash_report.log", $"[{DateTime.Now}] {ex}\n\n");
            }
            catch { }
        }
    }
}