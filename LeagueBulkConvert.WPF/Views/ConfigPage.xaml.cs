using System.Windows.Controls;
using LeagueBulkConvert.WPF.ViewModels;

namespace LeagueBulkConvert.WPF.Views
{
    partial class ConfigPage : Page
    {
        public ConfigPage(Config config)
        {
            InitializeComponent();
            DataContext = new ConfigPageViewModel(config, this);
        }
    }
}