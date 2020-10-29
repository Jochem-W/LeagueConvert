using LeagueBulkConvert.ViewModels;
using System.Windows;

namespace LeagueBulkConvert.Views
{
    partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
