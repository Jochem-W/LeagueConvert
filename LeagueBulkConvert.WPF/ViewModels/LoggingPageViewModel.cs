using LeagueBulkConvert.WPF.Views;
using Octokit;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
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
        public LoggingPageViewModel(Config config, System.Windows.Controls.Page owner) : this()
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
            task = Task.Run(async () =>
            {
                if (!Directory.Exists("hashes"))
                    Directory.CreateDirectory("hashes");
                IReadOnlyList<RepositoryContent> repositoryContents;
                try
                {
                    logger.Information("Downloading latest hashtables");
                    repositoryContents = await App.GitHubClient.Repository.Content.GetAllContents("CommunityDragon", "CDTB", "cdragontoolbox");
                    foreach (var file in repositoryContents.Where(f => f.Name == "hashes.binhashes.txt" || f.Name == "hashes.game.txt"))
                    {
                        var filePath = $"hashes/{file.Name}";
                        var shaFilePath = $"{filePath}.sha";
                        if (!File.Exists(filePath) || !File.Exists(shaFilePath) || await File.ReadAllTextAsync(shaFilePath) != file.Sha)
                        {
                            var tempFilePath = $"{filePath}.tmp";
                            await File.WriteAllTextAsync(tempFilePath, await App.HttpClient.GetStringAsync(file.DownloadUrl));
                            File.Move(tempFilePath, filePath);
                            await File.WriteAllTextAsync(shaFilePath, file.Sha);
                        }
                    }
                }
                catch (Exception)
                {
                    if (File.Exists("hashes/hashes.binhashes.txt") && File.Exists("hashes/hashes.game.txt"))
                        logger.Error("Couldn't update hashtables, using current version");
                    else
                    {
                        cancellationTokenSource.Cancel();
                        logger.Fatal("Couldn't download hashtables, cancelling!");
                        return;
                    }
                }
                await Utils.Convert(config, logger, cancellationTokenSource.Token);
            });
            task.ContinueWith(async (_) => await owner.Dispatcher.InvokeAsync(() =>
            {
                cancelCommand.RaiseCanExecuteChanged();
                previousCommand.RaiseCanExecuteChanged();
                cancellationTokenSource.Dispose();
            }));
        }
    }
}
