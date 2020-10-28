using LeagueBulkConvert.MVVM.ViewModels;
using System.Windows;

namespace LeagueBulkConvert.Windows
{
    partial class MaterialMessageBox : Window
    {
        public MaterialMessageBox(BoxViewModel viewModel) : base()
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        public MaterialMessageBox(BoxViewModel viewModel, Window owner) : base()
        {
            DataContext = viewModel;
            Owner = owner;
            InitializeComponent();
        }

        private void Ok(object sender, RoutedEventArgs e) => Close();
    }
}
