using LeagueBulkConvert.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace LeagueBulkConvert.Views
{
    partial class LoggingWindow : Window
    {
        private readonly LoggingViewModel loggingViewModel;

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            var viewModel = (LoggingViewModel)DataContext;
            if (e.ExtentHeightChange == 0)
            {
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                    viewModel.AutoScroll = true;
                else
                    viewModel.AutoScroll = false;
            }
            else if (viewModel.AutoScroll && e.ExtentHeightChange != 0)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonSaveFileDialog("Export log file") { DefaultFileName = "LeagueBulkConvert.log" };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                loggingViewModel.WriteToFile(dialog.FileName);
        }

        public LoggingWindow(LoggingViewModel viewModel, Window owner) : base()
        {
            InitializeComponent();
            loggingViewModel = viewModel;
            DataContext = loggingViewModel;
            Owner = owner;
        }
    }
}
