using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LeagueBulkConvert.WPF
{
    internal class ObservableWad : INotifyPropertyChanged
    {
        private readonly IncludableWad wad;

        public ObservableWad(IncludableWad wad)
        {
            this.wad = wad;
        }

        public bool Included
        {
            get => wad.Included;
            set
            {
                wad.Included = value;
                OnPropertyChanged();
            }
        }

        public string Name => wad.Name;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}