using LeagueBulkConvert.WPF.Views;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF.ViewModels
{
    class MainPageViewModel : INotifyPropertyChanged
    {
        public ICommand BrowseExport { get; }

        public ICommand BrowseLeague { get; }

        private string exportPath;
        public string ExportPath
        {
            get => exportPath;
            set
            {
                try
                {
                    exportPath = Path.GetFullPath(value);
                }
                catch (Exception)
                {
                    exportPath = null;
                }
                OnPropertyChanged();
                nextCommand.RaiseCanExecuteChanged();
            }
        }

        private string leaguePath;
        public string LeaguePath
        {
            get => leaguePath;
            set
            {
                var error = false;
                try
                {
                    leaguePath = Path.GetFullPath(value);
                    if (TryGetWadsPath(leaguePath, out string wadsPath))
                        this.wadsPath = wadsPath;
                    else
                    {
                        leaguePath = null;
                        this.wadsPath = null;
                    }
                }
                catch (Exception)
                {
                    leaguePath = null;
                    wadsPath = null;
                    error = true;
                }
                if ((leaguePath == null || wadsPath == null) && !error)
                    new MessageWindow("Invalid directory",
                        "Please select a valid League of Legends installation directory! (e.g. C:\\Riot Games\\League of Legends)").ShowDialog();
                OnPropertyChanged();
                nextCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly Command nextCommand;
        public ICommand NextCommand { get => nextCommand; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string wadsPath;

        private static string Browse(string initialDirectory)
        {
            using var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = initialDirectory,
                IsFolderPicker = true
            };
            return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
        }

        private void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private static bool TryGetWadsPath(string path, out string wadsPath)
        {
            wadsPath = Path.Combine(path, "Game", "DATA", "FINAL", "Champions");
            if (Directory.Exists(wadsPath))
                return true;
            var downloadPath = Path.Combine(path, "lol_game_client", "releases");
            var solutionPath = Path.Combine(path, "RADS", "projects", "lol_game_client", "releases");
            string pathPart;
            if (Directory.Exists(downloadPath))
            {
                pathPart = "files";
                wadsPath = downloadPath;
            }
            else if (Directory.Exists(solutionPath))
            {
                pathPart = "deploy";
                wadsPath = solutionPath;
            }
            else
                return false;
            IList<string> directories = Directory.GetDirectories(wadsPath);
            if (directories.Count == 0)
                return false;
            var newestVersion = new Version(Path.GetFileName(directories[0]));
            foreach (var versionString in directories.Skip(1))
            {
                var version = new Version(versionString);
                if (newestVersion.CompareTo(version) < 0)
                    newestVersion = version;
            }
            wadsPath = Path.Combine(wadsPath, newestVersion.ToString(), pathPart, "DATA", "FINAL", "Champions");
            return true;
        }

        public MainPageViewModel()
        {
            BrowseExport = new Command((_) => ExportPath = Browse(ExportPath));
            BrowseLeague = new Command((_) => LeaguePath = Browse(LeaguePath));
        }
        public MainPageViewModel(Page owner) : this()
        {
            nextCommand = new Command((_) =>
            {
                Directory.CreateDirectory(ExportPath);
                Directory.SetCurrentDirectory(ExportPath);
                var config = new Config();
                var t = Directory.EnumerateFiles(wadsPath, "*.wad.client");
                foreach (var filePath in Directory.EnumerateFiles(wadsPath, "*.wad.client")
                    .Where(f => Path.GetFileName(f).Count(c => c == '.') == 2))
                    config.Wads.Add(new IncludableWad(filePath));
                owner.NavigationService.Navigate(new ConfigPage(config));
            }, (_) => LeaguePath != null && ExportPath != null);
        }
    }
}
