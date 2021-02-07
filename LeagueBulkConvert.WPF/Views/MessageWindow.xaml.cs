using LeagueBulkConvert.WPF.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF.Views
{
    partial class MessageWindow : Window
    {
        public MessageWindow(string title, string message) : base()
        {
            InitializeComponent();
            DataContext = new MessageWindowViewModel(this)
            {
                Message = message,
                Title = title
            };
        }

        public MessageWindow(string title, string message, ICommand command) : base()
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
