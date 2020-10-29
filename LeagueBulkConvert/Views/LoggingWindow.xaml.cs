using System.Windows;
using System.Windows.Controls;

namespace LeagueBulkConvert.Views
{
    partial class LoggingWindow : Window
    {
        private bool autoScroll = true;

        public LoggingWindow() => InitializeComponent();

        private void ScrollViewer_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
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
    }
}
