using System;
using System.Windows;
using System.Windows.Controls;
using LeagueBulkConvert.WPF.ViewModels;

namespace LeagueBulkConvert.WPF.Views;

partial class LoggingPage : Page
{
    private readonly LoggingPageViewModel _viewModel;
    private bool _autoScroll = true;

    public LoggingPage(Config config)
    {
        InitializeComponent();
        _viewModel = new LoggingPageViewModel(config, this);
        DataContext = _viewModel;
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var scrollViewer = (ScrollViewer) sender;
        if (e.ExtentHeightChange == 0)
            _autoScroll = Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 0.1;
        else if (_autoScroll && e.ExtentHeightChange != 0)
            scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.Convert();
    }
}