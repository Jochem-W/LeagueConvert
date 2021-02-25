using System;
using System.Windows;
using System.Windows.Navigation;

namespace LeagueBulkConvert.WPF.Views
{
    partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigationService.Navigate(new MainPage());
        }

        private void NavigationWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}