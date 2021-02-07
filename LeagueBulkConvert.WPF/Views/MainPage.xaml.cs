using LeagueBulkConvert.WPF.ViewModels;
using System.Windows.Controls;

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
