using LeagueBulkConvert.ViewModels;
using LeagueBulkConvert.Views;
using System.IO;
using System.Windows;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using Octokit;
using System.Collections.Generic;

namespace LeagueBulkConvert
{
    partial class App : System.Windows.Application
    {
        internal static readonly GitHubClient GitHubClient = new GitHubClient(new ProductHeaderValue("LeagueBulkConvert"));

        internal static readonly HttpClient HttpClient = new HttpClient();

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!File.Exists("config.json") || !File.Exists("libzstd.dll"))
            {
                new MaterialMessageBox(new MaterialMessageBoxViewModel
                {
                    Message = "Please make sure that you've extracted the .zip file properly and are running the .exe correctly!"
                }).ShowDialog();
                Shutdown();
            }
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
                new MaterialMessageBox(new MaterialMessageBoxViewModel
                {
                    Message = $"Encountered the following error while checking for updates:\n" +
                    $"{exception.Message}\n" +
                    $"Are you connected to the internet?",
                    Title = "Update check failed!"
                }).ShowDialog();
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
                var messageBoxViewModel = new MaterialMessageBoxViewModel(new Command(_ => Process.Start(processStartInfo)))
                {
                    Message = "A new version of LeagueBulkConvert is available\n" +
                              "Clicking the 'Ok' button will take you to the downloads.",
                    Title = "Update available"
                };
                new MaterialMessageBox(messageBoxViewModel).ShowDialog();
            }
        }
    }
}
