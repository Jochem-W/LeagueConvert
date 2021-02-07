using LeagueBulkConvert.WPF.ViewModels;
using System.Windows.Controls;
using System.Windows.Data;

namespace LeagueBulkConvert.WPF.Views
{
    partial class WadsPage : Page
    {
        public WadsPage(Config config) : base()
        {
            InitializeComponent();
            DataContext = new WadsPageViewModel(config, this);
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Included")
            {
                ((DataGridCheckBoxColumn)e.Column).Binding = new Binding(e.PropertyName)
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
            }
        }
    }
}
