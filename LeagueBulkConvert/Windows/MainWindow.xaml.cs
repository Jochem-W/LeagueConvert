using LeagueBulkConvert.MVVM.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
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
            var process = new Process
            {
                StartInfo = new ProcessStartInfo($"{Environment.CurrentDirectory}\\config.json") { UseShellExecute = true }
            };
            process.Exited += (object sender, EventArgs e) => ((Process)sender).Dispose();
            process.Start();
        }
    }
}
