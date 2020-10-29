using System;
using System.Windows.Input;

namespace LeagueBulkConvert
{
    public class Command : ICommand
    {
        private Action<object> action;

        private Func<bool> predicate;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return predicate();
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, null);
        }

        public Command(Action<object> action, Func<bool> predicate)
        {
            this.action = action;
            this.predicate = predicate;
        }
    }
}
