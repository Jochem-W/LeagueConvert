using LeagueBulkConvert.Conversion;
using LeagueBulkConvert.Views;
using Microsoft.WindowsAPICodePack.Dialogs;
using Octokit;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LeagueBulkConvert.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool allowConversion;
        public bool AllowConversion
        {
            get => allowConversion;
            set
            {
                allowConversion = value;
                OnPropertyChanged();
                ConvertCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand BrowseLeague { get; }

        public ICommand BrowseOutput { get; }

        public Command ConvertCommand { get; }

        public ICommand EditConfigCommand { get; }

        private bool enableSkeletonCheckbox = true;
        public bool EnableSkeletonCheckbox
        {
            get => enableSkeletonCheckbox;
            set
            {
                enableSkeletonCheckbox = value;
                OnPropertyChanged();
            }
        }

        public GitHubClient GitHubClient = new GitHubClient(new ProductHeaderValue("LeagueBulkConvert"));

        public HttpClient HttpClient = new HttpClient();

        private bool includeAnimations;
        public bool IncludeAnimations
        {
            get => includeAnimations;
            set
            {
                includeAnimations = value;
                OnPropertyChanged();
                if (value)
                {
                    IncludeSkeletons = true;
                    EnableSkeletonCheckbox = false;
                    return;
                }
                EnableSkeletonCheckbox = true;
            }
        }

        public bool IncludeHiddenMeshes { get; set; }

        private bool includeSkeletons;
        public bool IncludeSkeletons
        {
            get => includeSkeletons;
            set
            {
                includeSkeletons = value;
                OnPropertyChanged();
            }
        }

        private Visibility loadingVisibility = Visibility.Hidden;
        public Visibility LoadingVisibility
        {
            get => loadingVisibility;
            set
            {
                loadingVisibility = value;
                OnPropertyChanged();
            }
        }

        private string leaguePath;
        public string LeaguePath
        {
            get => leaguePath;
            set
            {
                leaguePath = value;
                OnPropertyChanged();
                if (Directory.Exists(value) && Directory.Exists(OutPath))
                    AllowConversion = true;
                else
                    AllowConversion = false;
            }
        }

        private string outPath;
        public string OutPath
        {
            get => outPath;
            set
            {
                outPath = value;
                OnPropertyChanged();
                if (Directory.Exists(value) && Directory.Exists(OutPath))
                    AllowConversion = true;
                else
                    AllowConversion = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ReadVersion3 { get; set; } = true;

        public bool SaveAsGlTF { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private static string Browse(string initialDirectory)
        {
            if (string.IsNullOrWhiteSpace(initialDirectory))
                initialDirectory = "C:";
            using var dlg = new CommonOpenFileDialog
            {
                InitialDirectory = initialDirectory,
                IsFolderPicker = true
            };
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                return dlg.FileName;
            return string.Empty;
        }

        private async Task Convert()
        {
            try
            {
                AllowConversion = false;
                LoadingVisibility = Visibility.Visible;
                var viewModel = new LoggingWindowViewModel();
                new LoggingWindow(viewModel).Show();
                await Task.Run(async () => await Converter.StartConversion(this, viewModel));
                viewModel.AllowSave = true;
                LoadingVisibility = Visibility.Hidden;
                AllowConversion = true;
            }
            catch (Exception exception)
            {
                new MaterialMessageBox(new MaterialMessageBoxViewModel
                {
                    Message = exception.Message,
                    Title = "Error"
                }).ShowDialog();
            }
        }

        private static void EditConfig()
        {
            var process = new Process { StartInfo = new ProcessStartInfo("config.json") { UseShellExecute = true } };
            process.Exited += (object sender, EventArgs e) => ((Process)sender).Dispose();
            try
            {
                process.Start();
            }
            catch (Exception)
            {
                process.Dispose();
                process = new Process { StartInfo = new ProcessStartInfo("notepad.exe", "config.json") };
                process.Exited += (object sender, EventArgs e) => ((Process)sender).Dispose();
                try
                {
                    process.Start();
                }
                catch (Exception exception)
                {
                    process.Dispose();
                    new MaterialMessageBox(new MaterialMessageBoxViewModel
                    {
                        Message = $"Couldn't open config.json\n\n{exception.Message}",
                        Title = "Error"
                    }).ShowDialog();
                }
            }
        }

        private async Task CheckForUpdates()
        {
            var latestVersion = new Version((await GitHubClient.Repository.GetAllTags("Jochem-W", "LeagueBulkConvert"))[0].Name.Remove(0, 1));
            if (Assembly.GetExecutingAssembly().GetName().Version.CompareTo(latestVersion) < 0)
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
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => new MaterialMessageBox(messageBoxViewModel).ShowDialog());
            }
        }

        public MainWindowViewModel()
        {
            BrowseLeague = new Command(_ => LeaguePath = Browse(LeaguePath));
            BrowseOutput = new Command(_ => OutPath = Browse(OutPath));
            ConvertCommand = new Command(async _ => await Convert(), () => AllowConversion);
            EditConfigCommand = new Command(_ => EditConfig());
            Task.Run(CheckForUpdates);
        }
    }
}
