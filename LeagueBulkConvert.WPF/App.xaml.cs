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
        internal static readonly GitHubClient GitHubClient = new(new ProductHeaderValue("LeagueBulkConvert"));

        internal static readonly HttpClient HttpClient = new();

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await CheckForUpdates();
            new MainWindow().Show();
        }

        private static async Task CheckForUpdates()
        {
            IReadOnlyList<RepositoryTag> tags;
            try
            {
                tags = await GitHubClient.Repository.GetAllTags("Jochem-W", "LeagueBulkConvert");
            }
            catch (Exception exception)
            {
                new MessageWindow("Update check failed!",
                    "Encountered the following error while checking for updates:\n" +
                    $"{exception.Message}\n" +
                    "Are you connected to the internet?").ShowDialog();
                return;
            }

            var latestRelease = tags.First(t => !t.Name.Contains("pre"));
            var latestVersion = new Version(latestRelease.Name.Remove(0, 1));
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (currentVersion.CompareTo(latestVersion) < 0)
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "https://github.com/Jochem-W/LeagueBulkConvert/releases",
                    UseShellExecute = true
                };
                new MessageWindow("Update available", "A new version of LeagueBulkConvert is available\n" +
                                                      "Clicking the 'Ok' button will take you to the downloads.",
                    new Command(_ => Process.Start(processStartInfo))).ShowDialog();
            }
        }
    }
}