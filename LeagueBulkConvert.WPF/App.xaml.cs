using LeagueBulkConvert.WPF.Views;
using System.Windows;

namespace LeagueBulkConvert.WPF
{
    partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new MainWindow().Show();
        }
    }
}
