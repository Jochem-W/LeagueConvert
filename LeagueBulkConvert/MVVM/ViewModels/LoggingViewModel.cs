using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LeagueBulkConvert.MVVM.ViewModels
{
    class LoggingViewModel : INotifyPropertyChanged
    {
        public bool AutoScroll { get; set; }

        private string log;
        public string Log
        {
            get => log;
            set
            {
                log = value;
                OnPropertyChanged();
            }
        }

        public void AddLine(string line, int indent = 0)
        {
            Log += $"{new string(' ', 4 * indent)}{line}\n";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
