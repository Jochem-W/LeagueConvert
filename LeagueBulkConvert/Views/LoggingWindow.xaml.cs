using LeagueBulkConvert.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace LeagueBulkConvert.Views
{
    partial class LoggingWindow : Window
    {
        private bool autoScroll = true;

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (e.ExtentHeightChange == 0)
            {
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                    autoScroll = true;
                else
                    autoScroll = false;
            }
            else if (autoScroll && e.ExtentHeightChange != 0)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
        }

        public LoggingWindow(LoggingWindowViewModel viewModel) : base()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
