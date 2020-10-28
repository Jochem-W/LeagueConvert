using LeagueBulkConvert.MVVM.ViewModels;
using System.Windows;

namespace LeagueBulkConvert.Windows
{
    partial class PromptWindow : Window
    {
        public PromptWindow(PromptViewModel viewModel) : base()
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void Ok(object sender, RoutedEventArgs e) => Close();
    }
}
