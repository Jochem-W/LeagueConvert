using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using LeagueBulkConvert.WPF.Views;

namespace LeagueBulkConvert.WPF.ViewModels
{
    internal class ConfigPageViewModel : INotifyPropertyChanged
    {
        private readonly Config config = new();

        public ConfigPageViewModel()
        {
        }

        public ConfigPageViewModel(Config config, Page owner)
        {
            this.config = config;
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
            get => config.IncludeAnimations;
            set
            {
                config.IncludeAnimations = value;
                if (value)
                    IncludeSkeleton = true;
                OnPropertyChanged();
            }
        }

        public bool IncludeHiddenMeshes
        {
            get => config.IncludeHiddenMeshes;
            set => config.IncludeHiddenMeshes = value;
        }

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

        public bool ReadVersion3
        {
            get => config.ReadVersion3;
            set => config.ReadVersion3 = value;
        }

        public bool SaveAsGlTF
        {
            get => config.SaveAsGlTF;
            set => config.SaveAsGlTF = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}