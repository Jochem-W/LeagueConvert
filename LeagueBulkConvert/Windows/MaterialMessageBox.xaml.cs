using System.Windows;

namespace LeagueBulkConvert.Windows
{
    partial class MaterialMessageBox : Window
    {
        public MaterialMessageBox() => InitializeComponent();

        private void Ok(object sender, RoutedEventArgs e) => Close();
    }
}
