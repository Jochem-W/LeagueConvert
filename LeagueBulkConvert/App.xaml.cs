using LeagueBulkConvert.Views;
using System.IO;
using System.Reflection;
using System.Windows;

namespace LeagueBulkConvert
{
    partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            new MainWindow().Show();
        }
    }
}
