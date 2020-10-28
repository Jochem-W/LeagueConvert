using System.Windows;

namespace LeagueBulkConvert.Windows
{
    partial class PromptWindow : Window
    {
        public PromptWindow() => InitializeComponent();

        private void Ok(object sender, RoutedEventArgs e) => Close();
    }
}
