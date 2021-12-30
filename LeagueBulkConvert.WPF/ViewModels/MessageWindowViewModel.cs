using System.Windows;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF.ViewModels;

public class MessageWindowViewModel
{
    public MessageWindowViewModel()
    {
    }

    public MessageWindowViewModel(Window owner)
    {
        Ok = new Command(_ => owner.Close());
    }

    public MessageWindowViewModel(Window owner, ICommand command)
    {
        Ok = new Command(_ =>
        {
            command.Execute(_);
            owner.Close();
        });
    }

    public ICommand Ok { get; }

    public string ButtonText { get; set; } = "Ok";

    public string Message { get; set; }

    public string Title { get; set; } = "LeagueBulkConvert";
}