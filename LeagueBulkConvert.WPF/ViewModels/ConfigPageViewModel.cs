using LeagueBulkConvert.WPF.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF.ViewModels
{
    class ConfigPageViewModel : INotifyPropertyChanged
    {
        private readonly Config config = new Config();

        public bool IncludeAnimations
        {
            get => config.IncludeAnimations;
            set
            {
                config.IncludeAnimations = value;
                if (value)
                    IncludeSkeleton = true;
                OnPropertyChanged();
            }
        }

        public bool IncludeHiddenMeshes { get => config.IncludeHiddenMeshes; set => config.IncludeHiddenMeshes = value; }

        public bool IncludeSkeleton
        {
            get => config.IncludeSkeleton;
            set
            {
                config.IncludeSkeleton = value;
                if (!value)
                    IncludeAnimations = false;
                OnPropertyChanged();
            }
        }

        public ICommand NextCommand { get; }

        public ICommand PreviousCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public bool ReadVersion3 { get => config.ReadVersion3; set => config.ReadVersion3 = value; }

        public bool SaveAsGlTF { get => config.SaveAsGlTF; set => config.SaveAsGlTF = value; }

        public ConfigPageViewModel() { }
        public ConfigPageViewModel(Config config, Page owner)
        {
            this.config = config;
            NextCommand = new Command((_) => owner.NavigationService.Navigate(new WadsPage(config)));
            PreviousCommand = new Command((_) => owner.NavigationService.GoBack());
        }
    }
}
