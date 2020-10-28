using LeagueBulkConvert.MVVM.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace LeagueBulkConvert.Windows
{
    partial class LoggingWindow : Window
    {
        public LoggingWindow() => InitializeComponent();

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
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
            }
        }
    }
}
