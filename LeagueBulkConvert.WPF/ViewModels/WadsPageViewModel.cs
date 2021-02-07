using LeagueBulkConvert.WPF.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF.ViewModels
{
    class WadsPageViewModel
    {
        private readonly Command nextCommand;
        public ICommand NextCommand { get => nextCommand; }

        public ICommand PreviousCommand { get; }

        public ObservableCollection<ObservableWad> Wads { get; } = new ObservableCollection<ObservableWad>();

        public WadsPageViewModel() { }
        public WadsPageViewModel(Config config, Page owner)
        {
            Wads.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    foreach (INotifyPropertyChanged item in e.NewItems)
                        item.PropertyChanged += (object sender, PropertyChangedEventArgs e) => nextCommand.RaiseCanExecuteChanged();
            };
            foreach (var wad in config.Wads)
                Wads.Add(new ObservableWad(wad));
            nextCommand = new Command((_) => owner.NavigationService.Navigate(new LoggingPage(config)), (_) => Wads.Any(w => w.Included));
            PreviousCommand = new Command((_) => owner.NavigationService.GoBack());
        }
    }
}
