using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace LeagueBulkConvert.WPF.ViewModels;

internal class LoggingPageViewModel : INotifyPropertyChanged
{
    private readonly Command _cancelCommand;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly Config _config;

    private readonly ObservableCollection<string> _log = new();

    private readonly Logger _logger;

    private readonly Command _previousCommand;

    private bool _completed;

    public LoggingPageViewModel()
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(LogEventLevel.Information)
            .CreateLogger();
        Console.SetOut(new ListWriter(_log));
        _cancelCommand = new Command(_ =>
        {
            _logger.Information("Requesting cancellation");
            _cancellationTokenSource.Cancel();
            _cancelCommand.RaiseCanExecuteChanged();
        }, _ => !_cancellationTokenSource.IsCancellationRequested && !_completed);
        _log.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Log));
    }

    public LoggingPageViewModel(Config config, Page owner) : this()
    {
        _config = config;
        _previousCommand = new Command(_ => owner.NavigationService.GoBack(), _ => _completed);
    }

    public ICommand CancelCommand => _cancelCommand;
    public string Log => string.Join('\n', _log.TakeLast(256));
    public ICommand PreviousCommand => _previousCommand;

    public event PropertyChangedEventHandler PropertyChanged;

    internal async Task Convert()
    {
        App.AllowNavigation = false;
        if (!Directory.Exists("hashes"))
            Directory.CreateDirectory("hashes");


        if (await HashTables.TryLoad(_logger))
            await Task.Run(async () => await Utils.Convert(_config, _logger, _cancellationTokenSource.Token));
        _completed = true;
        App.AllowNavigation = true;
        _cancelCommand.RaiseCanExecuteChanged();
        _previousCommand.RaiseCanExecuteChanged();
        _cancellationTokenSource.Dispose();
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}