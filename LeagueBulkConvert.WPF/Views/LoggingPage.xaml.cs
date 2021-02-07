using LeagueBulkConvert.WPF.ViewModels;
using System.Windows.Controls;

namespace LeagueBulkConvert.WPF.Views
{
    partial class LoggingPage : Page
    {
        private bool autoScroll = true;

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
            DataContext = new LoggingPageViewModel(config, this);
        }
    }
}
