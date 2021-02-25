using System.Windows.Controls;
using LeagueBulkConvert.WPF.ViewModels;

namespace LeagueBulkConvert.WPF.Views
{
    partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            DataContext = new MainPageViewModel(this);
        }
    }
}