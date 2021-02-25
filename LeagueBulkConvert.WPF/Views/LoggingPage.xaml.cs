using LeagueBulkConvert.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace LeagueBulkConvert.WPF.Views
{
    partial class LoggingPage : Page
    {
        private bool autoScroll = true;

        private readonly LoggingPageViewModel viewModel;

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (e.ExtentHeightChange == 0)
                autoScroll = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight;
            else if (autoScroll && e.ExtentHeightChange != 0)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
        }

        public LoggingPage(Config config)
        {
            InitializeComponent();
            viewModel = new LoggingPageViewModel(config, this);
            DataContext = viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e) => await viewModel.Convert();
    }
}
