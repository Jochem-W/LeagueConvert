using LeagueBulkConvert.WPF.ViewModels;
using System.Windows.Controls;

namespace LeagueBulkConvert.WPF.Views
{
    partial class ConfigPage : Page
    {
        public ConfigPage(Config config) : base()
        {
            InitializeComponent();
            DataContext = new ConfigPageViewModel(config, this);
        }
    }
}
