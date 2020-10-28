using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace LeagueBulkConvert.Windows
{
    partial class LinkMessageBox : Window
    {
        public LinkMessageBox() => InitializeComponent();

        public LinkMessageBox(string text1) : base()
        {
            InitializeComponent();
            TextBlock.Inlines.Add($"{text1}");
        }

        public LinkMessageBox(string text1, Uri uri, string text2) : base()
        {
            InitializeComponent();
            TextBlock.Inlines.Add($"{text1}");
            var hyperLink = new Hyperlink()
            {
                NavigateUri = uri
            };
            hyperLink.Inlines.Add(uri.AbsoluteUri);
            hyperLink.RequestNavigate += (object sender, RequestNavigateEventArgs e) =>
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true })
                       .Exited += (object sender, EventArgs e) => ((Process)sender).Dispose();
            };
            TextBlock.Inlines.Add(hyperLink);
            TextBlock.Inlines.Add($"\n\n{text2}");
        }

        private void Ok(object sender, RoutedEventArgs e) => Close();
    }
}
