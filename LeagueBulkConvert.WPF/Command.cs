using System;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF
{
    class Command : ICommand
    {
        private readonly Predicate<object> predicate;

        public event EventHandler CanExecuteChanged;

        private readonly Action<object> action;

        public bool CanExecute(object parameter) => predicate(parameter);

        public void Execute(object parameter) => action(parameter);

        public Command(Action<object> action)
        {
            this.action = action;
            predicate = (_) => true;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged(this, null);

        public Command(Action<object> action, Predicate<object> predicate)
        {
            this.action = action;
            this.predicate = predicate;
        }
    }
}
