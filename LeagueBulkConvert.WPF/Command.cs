using System;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF
{
    internal class Command : ICommand
    {
        private readonly Action<object> action;
        private readonly Predicate<object> predicate;

        public Command(Action<object> action)
        {
            this.action = action;
            predicate = _ => true;
        }

        public Command(Action<object> action, Predicate<object> predicate)
        {
            this.action = action;
            this.predicate = predicate;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return predicate(parameter);
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, null);
        }
    }
}