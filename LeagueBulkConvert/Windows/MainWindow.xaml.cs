using LeagueBulkConvert.MVVM.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
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

        private void BrowseLeague(object sender, RoutedEventArgs e) =>
            viewModel.LeaguePath = Browse(viewModel.LeaguePath);

        private void BrowseOutput(object sender, RoutedEventArgs e) => viewModel.OutPath = Browse(viewModel.OutPath);

        private async void Convert(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            senderButton.IsEnabled = false;
            viewModel.LoadingVisibility = Visibility.Visible;
            var loggingViewModel = new LoggingViewModel();
            new LoggingWindow(loggingViewModel, this).Show();
            await Task.Run(async () => await Conversion.Converter.StartConversion(viewModel, loggingViewModel));
            viewModel.LoadingVisibility = Visibility.Hidden;
            senderButton.IsEnabled = true;
        }

        private void EditConfig(object sender, RoutedEventArgs e)
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
                    }, this).ShowDialog();
                }
            }
        }
    }
}
