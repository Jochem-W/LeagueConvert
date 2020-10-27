using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LeagueBulkConvert.MVVM.ViewModels
{
    class LoggingViewModel : INotifyPropertyChanged
    {
        public void AddLine(string text, int indent = 0)
        {
            lines.Add(new string(' ', 4 * indent) + text);
        }

        public bool AutoScroll { get; set; } = true;

        private readonly ObservableCollection<string> lines;

        public string Log { get => string.Join('\n', lines.TakeLast(256)); }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public LoggingViewModel()
        {
            lines = new ObservableCollection<string>();
            lines.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged("Log");
        }
    }
}
