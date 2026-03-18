using System.Configuration;
using System.Data;
using System.Windows;

namespace SimNite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += (s, ev) => 
                System.IO.File.WriteAllText("crash.log", ev.ExceptionObject.ToString());
            DispatcherUnhandledException += (s, ev) => 
            {
                System.IO.File.WriteAllText("crash_dispatcher.log", ev.Exception.ToString());
            };
        }
    }
}
