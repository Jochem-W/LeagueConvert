using System;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF;

internal class Command : ICommand
{
    private readonly Action<object> _action;
    private readonly Predicate<object> _predicate;

    public Command(Action<object> action)
    {
        _action = action;
        _predicate = _ => true;
    }

    public Command(Action<object> action, Predicate<object> predicate)
    {
        _action = action;
        _predicate = predicate;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return _predicate(parameter);
    }

    public void Execute(object parameter)
    {
        _action(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged(this, null);
    }
}