using System;
using System.Windows.Input;

namespace LeagueBulkConvert
{
    public class Command : ICommand
    {
        private readonly Action<object> action;

        private readonly Func<bool> predicate;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => predicate();

        public void Execute(object parameter) => action(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged(this, null);

        public Command(Action<object> action)
        {
            this.action = action;
            predicate = () => true;
        }
        public Command(Action<object> action, Func<bool> predicate)
        {
            this.action = action;
            this.predicate = predicate;
        }
    }
}
