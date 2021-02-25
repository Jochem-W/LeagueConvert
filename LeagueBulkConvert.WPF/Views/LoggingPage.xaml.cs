using System.Windows;
using System.Windows.Controls;
using LeagueBulkConvert.WPF.ViewModels;

namespace LeagueBulkConvert.WPF.Views
{
    partial class LoggingPage : Page
    {
        private readonly LoggingPageViewModel viewModel;
        private bool autoScroll = true;

        public LoggingPage(Config config)
        {
            InitializeComponent();
            viewModel = new LoggingPageViewModel(config, this);
            DataContext = viewModel;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer) sender;
            if (e.ExtentHeightChange == 0)
                autoScroll = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight;
            else if (autoScroll && e.ExtentHeightChange != 0)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.Convert();
        }
    }
}