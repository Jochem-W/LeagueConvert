using LeagueBulkConvert.WPF.Views;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
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
                try
                {
                    var fullPath = Path.GetFullPath(value);
                    leaguePath = Directory.Exists(Path.Combine(fullPath, "Game", "DATA", "FINAL", "Champions")) ? fullPath : null;
                }
                catch (Exception)
                {
                    leaguePath = null;
                }
                OnPropertyChanged();
                nextCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly Command nextCommand;
        public ICommand NextCommand { get => nextCommand; }

        public event PropertyChangedEventHandler PropertyChanged;

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
                foreach (var file in Directory.EnumerateFiles(Path.Combine(LeaguePath, "Game", "DATA", "FINAL", "Champions"), "*.wad.client")
                    .Where(f => f.Count(c => c == '.') == 2))
                    config.Wads.Add(new IncludableWad(file));
                owner.NavigationService.Navigate(new ConfigPage(config));
            }, (_) => LeaguePath != null && ExportPath != null);
        }
    }
}
