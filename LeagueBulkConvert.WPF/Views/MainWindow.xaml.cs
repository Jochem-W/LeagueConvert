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
    }
}
