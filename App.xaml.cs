using System.Configuration;
using System.Data;
using System.Windows;
using System.Text.Json;
using AutoUpdaterDotNET;

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

            // Forcer la langue en Anglais pour la fenêtre de mise à jour
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            // INITIALISER L'AUTO-UPDATER GITHUB
            // /!\ Attention : Remplacez 'VOTRE_NOM_GITHUB' par votre vrai pseudo GitHub /!\
            AutoUpdater.HttpUserAgent = "SimNite-Updater";
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            AutoUpdater.Start("https://api.github.com/repos/erwang64/SimNite/releases/latest");

            AppDomain.CurrentDomain.UnhandledException += (s, ev) => 
                System.IO.File.WriteAllText("crash.log", ev.ExceptionObject.ToString());
            DispatcherUnhandledException += (s, ev) => 
            {
                System.IO.File.WriteAllText("crash_dispatcher.log", ev.Exception.ToString());
            };
        }

        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            try
            {
                using var doc = JsonDocument.Parse(args.RemoteData);
                var root = doc.RootElement;
                
                string version = root.GetProperty("tag_name").GetString() ?? "";
                if (version.StartsWith("v")) version = version.Substring(1); // Enlever le 'v' (ex: v1.0.1 -> 1.0.1)

                string downloadUrl = "";
                // Trouver le premier fichier attaché (Asset) à la release (votre .zip ou setup.exe)
                if (root.TryGetProperty("assets", out var assets) && assets.GetArrayLength() > 0)
                {
                    downloadUrl = assets[0].GetProperty("browser_download_url").GetString() ?? "";
                }
                
                string changelogUrl = root.GetProperty("html_url").GetString() ?? "";

                args.UpdateInfo = new UpdateInfoEventArgs
                {
                    CurrentVersion = version,
                    ChangelogURL = changelogUrl,
                    DownloadURL = downloadUrl
                };
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("updater_error.log", $"Erreur de parsing de l'updater: {ex.Message}");
            }
        }
    }
}
