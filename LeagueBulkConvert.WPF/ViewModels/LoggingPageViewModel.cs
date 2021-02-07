using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace LeagueBulkConvert.WPF.ViewModels
{
    class LoggingPageViewModel : ILogEventSink, INotifyPropertyChanged
    {
        private readonly Command cancelCommand;
        public ICommand CancelCommand { get => cancelCommand; }

        private readonly ObservableCollection<string> log = new ObservableCollection<string>();
        public string Log { get => string.Join('\n', log.TakeLast(256)); }

        private readonly Command previousCommand;
        public ICommand PreviousCommand { get => previousCommand; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Emit(LogEvent logEvent) => log.Add(logEvent.RenderMessage());

        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly Task task;

        public LoggingPageViewModel() => log.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged("Log");
        public LoggingPageViewModel(Config config, Page owner) : this()
        {
            previousCommand = new Command((_) => owner.NavigationService.GoBack(), (_) => task.IsCompleted);
            var cancellationTokenSource = new CancellationTokenSource();
            using var logger = new LoggerConfiguration()
                .WriteTo.Sink(this)
                .CreateLogger();
            cancelCommand = new Command((_) =>
            {
                logger.Information("Requesting cancellation");
                cancellationTokenSource.Cancel();
            }, (_) => !task.IsCompleted);
            task = Task.Run(async () => await Utils.Convert(config, logger, cancellationTokenSource.Token));
            task.ContinueWith(async (_) => await owner.Dispatcher.InvokeAsync(() =>
            {
                cancelCommand.RaiseCanExecuteChanged();
                previousCommand.RaiseCanExecuteChanged();
            }));
        }
    }
}
