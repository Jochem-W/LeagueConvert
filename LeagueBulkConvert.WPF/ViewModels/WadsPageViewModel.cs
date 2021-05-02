using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using LeagueBulkConvert.WPF.Views;

namespace LeagueBulkConvert.WPF.ViewModels
{
    internal class WadsPageViewModel : INotifyPropertyChanged
    {
        private readonly Command _nextCommand;

        private string _selectContent = "Select all";

        public WadsPageViewModel()
        {
            SelectCommand = new Command(_ =>
            {
                if (Wads.All(w => w.Included))
                    foreach (var wad in Wads)
                        wad.Included = false;
                else
                    foreach (var wad in Wads.Where(w => !w.Included))
                        wad.Included = true;
            });
            Wads.CollectionChanged += (sender, e) =>
            {
                if (e.Action != NotifyCollectionChangedAction.Add) return;
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += (_, _) =>
                    {
                        _nextCommand.RaiseCanExecuteChanged();
                        if (Wads.All(w => w.Included))
                            SelectContent = "Deselect all";
                        else if (SelectContent != "Select all")
                            SelectContent = "Select all";
                    };
            };
        }

        public WadsPageViewModel(Config config, Page owner) : this()
        {
            foreach (var wad in config.Wads)
                Wads.Add(new ObservableWad(wad));
            _nextCommand = new Command(_ => owner.NavigationService.Navigate(new LoggingPage(config)),
                _ => Wads.Any(w => w.Included));
            PreviousCommand = new Command(_ => owner.NavigationService.GoBack());
        }

        public ICommand NextCommand => _nextCommand;

        public ICommand PreviousCommand { get; }

        public ICommand SelectCommand { get; }

        public string SelectContent
        {
            get => _selectContent;
            set
            {
                _selectContent = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ObservableWad> Wads { get; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}