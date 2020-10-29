using LeagueBulkConvert.Views;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LeagueBulkConvert.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public ICommand BrowseLeague { get; }

        public ICommand BrowseOutput { get; }

        public Command ConvertCommand { get; }

        public ICommand EditConfigCommand { get; }

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

        public Window Owner { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ShowErrors { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string Browse(string initialDirectory)
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
            AllowConversion = false;
            LoadingVisibility = Visibility.Visible;
            var loggingViewModel = new LoggingViewModel();
            new LoggingWindow(loggingViewModel, Owner).Show();
            await Task.Run(async () => await Conversion.Converter.StartConversion(this, loggingViewModel));
            loggingViewModel.AllowSave = true;
            LoadingVisibility = Visibility.Hidden;
            AllowConversion = true;
        }

        private void EditConfig()
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
                    new MaterialMessageBox(new BoxViewModel
                    {
                        Message = $"Couldn't open config.json\n\n{exception.StackTrace}",
                        Title = "Error"
                    }).ShowDialog();
                }
            }
        }

        public MainViewModel()
        {
            BrowseLeague = new Command(p => LeaguePath = Browse(LeaguePath), () => true);
            BrowseOutput = new Command(p => OutPath = Browse(OutPath), () => true);
            ConvertCommand = new Command(async p => await Convert(), () => AllowConversion);
            EditConfigCommand = new Command(p => EditConfig(), () => true);
        }

        public MainViewModel(Window owner) : base() => Owner = owner;
    }
}
