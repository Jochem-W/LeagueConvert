using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LeagueBulkConvert.MVVM.ViewModels
{
    public class LoggingViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<string> lines = new ObservableCollection<string>();

        public bool AutoScroll { get; set; } = true;

        public string Log { get => string.Join('\n', lines.TakeLast(256)); }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddLine(string text, int indent = 0) => lines.Add(new string(' ', 4 * indent) + text);

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async void WriteToFile(string filename)
        {
            var writer = File.CreateText(filename);
            foreach (var line in lines)
                await writer.WriteLineAsync(line);
            await writer.DisposeAsync();
        }

        public LoggingViewModel() =>
            lines.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged("Log");
    }
}
