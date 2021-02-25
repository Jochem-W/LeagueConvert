using System.Windows;
using System.Windows.Input;
using LeagueBulkConvert.WPF.ViewModels;

namespace LeagueBulkConvert.WPF.Views
{
    partial class MessageWindow : Window
    {
        public MessageWindow(string title, string message)
        {
            InitializeComponent();
            DataContext = new MessageWindowViewModel(this)
            {
                Message = message,
                Title = title
            };
        }

        public MessageWindow(string title, string message, ICommand command)
        {
            InitializeComponent();
            DataContext = new MessageWindowViewModel(this, command)
            {
                Message = message,
                Title = title
            };
        }
    }
}