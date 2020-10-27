using LeagueBulkConvert.MVVM.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LeagueBulkConvert.Windows
{
    partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
        }

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

        private void BrowseLeague(object sender, RoutedEventArgs e)
        {
            viewModel.LeaguePath = Browse(viewModel.LeaguePath);
        }

        private void BrowseOutput(object sender, RoutedEventArgs e)
        {
            viewModel.OutPath = Browse(viewModel.OutPath);
        }

        private async void Convert(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            viewModel.LoadingVisibility = Visibility.Visible;
            await Task.Run(async () => await Converter.Converter.StartConversion(viewModel.LeaguePath, viewModel.OutPath, viewModel.IncludeSkeletons, viewModel.IncludeAnimations));
            viewModel.LoadingVisibility = Visibility.Hidden;
            ((Button)sender).IsEnabled = true;
        }

        private void EditConfig(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("config.json"))
            {
                MessageBox.Show("Couldn't find config.json. Did you extract the archive correctly?");
                return;
            }
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("config.json") { UseShellExecute = true }
            };
            process.Exited += (object sender, EventArgs e) => ((Process)sender).Dispose();
            try
            {
                process.Start();
            }
            catch (Win32Exception exception)
            {
                MessageBox.Show($"{exception.Message}");
                process.Dispose();
            }
        }
    }
}
