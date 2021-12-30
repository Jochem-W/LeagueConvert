using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LeagueBulkConvert.WPF;

internal class ObservableWad : INotifyPropertyChanged
{
    private readonly IncludableWad _wad;

    public ObservableWad(IncludableWad wad)
    {
        _wad = wad;
    }

    public bool Included
    {
        get => _wad.Included;
        set
        {
            _wad.Included = value;
            OnPropertyChanged();
        }
    }

    public string Name => _wad.Name;

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}