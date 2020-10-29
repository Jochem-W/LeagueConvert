using LeagueBulkConvert.ViewModels;
using System.Windows;

namespace LeagueBulkConvert.Views
{
    partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel(this);
            InitializeComponent();
        }
    }
}
