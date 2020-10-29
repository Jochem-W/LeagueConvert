using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LeagueBulkConvert.ViewModels
{
    public class LoggingWindowViewModel : INotifyPropertyChanged
    {
        public Command SaveCommand { get; }

        private bool allowSave = false;
        public bool AllowSave
        {
            get => allowSave;
            set
            {
                allowSave = value;
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly ObservableCollection<string> lines = new ObservableCollection<string>();
        public string Log { get => string.Join('\n', lines.TakeLast(256)); }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddLine(string text, int indent = 0) => lines.Add(new string(' ', 4 * indent) + text);

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private async Task Save()
        {
            var dialog = new CommonSaveFileDialog("Save log file")
            {
                DefaultExtension = "log",
                DefaultFileName = "LeagueBulkConvert"
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var writer = File.CreateText(dialog.FileName);
                foreach (var line in lines)
                    await writer.WriteLineAsync(line);
                await writer.DisposeAsync();
            }
        }

        public LoggingWindowViewModel()
        {
            lines.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged("Log");
            SaveCommand = new Command(async p => await Save(), () => AllowSave);
        }
    }
}
