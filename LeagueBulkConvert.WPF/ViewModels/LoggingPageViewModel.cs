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

namespace LeagueBulkConvert.WPF.ViewModels
{
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
            try
            {
                _logger.Information("Downloading latest hashtables");
                var repositoryContents = await App.GitHubClient.Repository.Content.GetAllContents("CommunityDragon",
                    "CDTB",
                    "cdragontoolbox");
                foreach (var file in repositoryContents.Where(f =>
                    f.Name is "hashes.binhashes.txt" or "hashes.game.txt"))
                {
                    var filePath = $"hashes/{file.Name}";
                    var shaFilePath = $"{filePath}.sha";
                    if (File.Exists(filePath) && File.Exists(shaFilePath) &&
                        await File.ReadAllTextAsync(shaFilePath) == file.Sha) continue;
                    var tempFilePath = $"{filePath}.tmp";
                    await File.WriteAllTextAsync(tempFilePath,
                        await App.HttpClient.GetStringAsync(file.DownloadUrl));
                    File.Move(tempFilePath, filePath);
                    await File.WriteAllTextAsync(shaFilePath, file.Sha);
                }
            }
            catch (Exception)
            {
                if (File.Exists("hashes/hashes.binhashes.txt") && File.Exists("hashes/hashes.game.txt"))
                {
                    _logger.Error("Couldn't update hashtables, using current version");
                }
                else
                {
                    _logger.Fatal("Couldn't download hashtables, cancelling!");
                    _cancellationTokenSource.Cancel();
                }
            }

            if (!_cancellationTokenSource.IsCancellationRequested)
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
}