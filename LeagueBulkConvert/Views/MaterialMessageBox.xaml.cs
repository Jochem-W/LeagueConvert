using LeagueBulkConvert.ViewModels;
using System.Windows;

namespace LeagueBulkConvert.Views
{
    partial class MaterialMessageBox : Window
    {
        public MaterialMessageBox(BoxViewModel viewModel) : base()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public MaterialMessageBox(BoxViewModel viewModel, Window owner) : base()
        {
            InitializeComponent();
            DataContext = viewModel;
            Owner = owner;
        }

        private void Ok(object sender, RoutedEventArgs e) => Close();
    }
}
