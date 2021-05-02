using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using LeagueBulkConvert.WPF.Views;
using Octokit;
using Application = System.Windows.Application;

namespace LeagueBulkConvert.WPF
{
    partial class App : Application
    {
        internal static bool AllowNavigation { get; set; } = true;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await TryCheckForUpdates();
            new MainWindow().Show();
        }

        private static async Task<bool> TryCheckForUpdates()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly().GetName();
                var httpClient = new HttpClient();
                var name = assembly.Name?.ToLower();
                var latestVersion = new Version(await httpClient.GetStringAsync(
                    $"https://api.jochemw.workers.dev/products/{name}/version/latest"));
                httpClient.Dispose();
                if (!(assembly.Version?.CompareTo(latestVersion) < 0))
                    return true;
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = $"https://api.jochemw.workers.dev/products/{name}/",
                    UseShellExecute = true
                };
                new MessageWindow("Update available", "A new version of LeagueBulkConvert is available\n" +
                                                      "Clicking the 'Ok' button will take you to the downloads",
                    new Command(_ => Process.Start(processStartInfo))).ShowDialog();
                return true;
            }
            catch (Exception e)
            {
                new MessageWindow("Update check failed!",
                    "Encountered the following error while checking for updates:\n" +
                    $"{e.Message}").ShowDialog();
                return false;
            }
        }
    }
}