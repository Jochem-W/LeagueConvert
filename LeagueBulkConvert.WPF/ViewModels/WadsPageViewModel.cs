using LeagueBulkConvert.WPF.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF.ViewModels
{
    class WadsPageViewModel : INotifyPropertyChanged
    {
        private readonly Command nextCommand;
        public ICommand NextCommand { get => nextCommand; }

        public ICommand PreviousCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SelectCommand { get; }

        private string selectContent = "Select all";
        public string SelectContent
        {
            get => selectContent;
            set
            {
                selectContent = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ObservableWad> Wads { get; } = new ObservableCollection<ObservableWad>();

        private void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public WadsPageViewModel()
        {
            SelectCommand = new Command((_) =>
            {
                if (!Wads.Any(w => !w.Included))
                    foreach (var wad in Wads)
                        wad.Included = false;
                else
                    foreach (var wad in Wads.Where(w => !w.Included))
                        wad.Included = true;
            });
            Wads.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    foreach (INotifyPropertyChanged item in e.NewItems)
                        item.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
                        {
                            nextCommand.RaiseCanExecuteChanged();
                            if (!Wads.Any(w => !w.Included))
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
            nextCommand = new Command((_) => owner.NavigationService.Navigate(new LoggingPage(config)), (_) => Wads.Any(w => w.Included));
            PreviousCommand = new Command((_) => owner.NavigationService.GoBack());
        }
    }
}
