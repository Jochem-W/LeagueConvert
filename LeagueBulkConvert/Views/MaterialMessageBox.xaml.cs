using LeagueBulkConvert.ViewModels;
using System.Windows;

namespace LeagueBulkConvert.Views
{
    partial class MaterialMessageBox : Window
    {
        public MaterialMessageBox(MaterialMessageBoxViewModel viewModel) : base()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
