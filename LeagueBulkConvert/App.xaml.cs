using LeagueBulkConvert.ViewModels;
using LeagueBulkConvert.Views;
using System.IO;
using System.Windows;

namespace LeagueBulkConvert
{
    partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!File.Exists("config.json") || !File.Exists("libzstd.dll") || !File.Exists("Magick.Native-Q16-x64.dll"))
            {
                new MaterialMessageBox(new MaterialMessageBoxViewModel
                {
                    Message = "Please make sure that you've extracted the .zip file properly and are running the .exe correctly!"
                }).ShowDialog();
                Shutdown();
            }
            new MainWindow().Show();
        }
    }
}
