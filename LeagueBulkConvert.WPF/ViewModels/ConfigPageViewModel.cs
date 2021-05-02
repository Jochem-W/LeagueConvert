using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using LeagueBulkConvert.WPF.Views;

namespace LeagueBulkConvert.WPF.ViewModels
{
    internal class ConfigPageViewModel : INotifyPropertyChanged
    {
        private readonly Config _config = new();

        public ConfigPageViewModel()
        {
        }

        public ConfigPageViewModel(Config config, Page owner)
        {
            _config = config;
            NextCommand = new Command(_ =>
            {
                if (owner.NavigationService.CanGoForward)
                    owner.NavigationService.GoForward();
                else
                    owner.NavigationService.Navigate(new WadsPage(config));
            });
            PreviousCommand = new Command(_ => owner.NavigationService.GoBack());
        }

        public bool IncludeAnimations
        {
            get => _config.IncludeAnimations;
            set
            {
                _config.IncludeAnimations = value;
                if (value)
                    IncludeSkeleton = true;
                OnPropertyChanged();
            }
        }

        public bool IncludeHiddenMeshes
        {
            get => _config.IncludeHiddenMeshes;
            set => _config.IncludeHiddenMeshes = value;
        }

        public bool IncludeSkeleton
        {
            get => _config.IncludeSkeleton;
            set
            {
                _config.IncludeSkeleton = value;
                if (!value)
                    IncludeAnimations = false;
                OnPropertyChanged();
            }
        }

        public ICommand NextCommand { get; }

        public ICommand PreviousCommand { get; }

        public bool SaveAsGlTf
        {
            get => _config.SaveAsGlTf;
            set => _config.SaveAsGlTf = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}