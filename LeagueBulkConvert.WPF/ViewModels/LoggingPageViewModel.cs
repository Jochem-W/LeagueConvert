using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Octokit;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Page = System.Windows.Controls.Page;

namespace LeagueBulkConvert.WPF.ViewModels
{
    internal class LoggingPageViewModel : INotifyPropertyChanged
    {
        private readonly Command cancelCommand;

        private readonly CancellationTokenSource cancellationTokenSource = new();

        private readonly Config config;

        private readonly ObservableCollection<string> log = new();

        private readonly Logger logger;

        private readonly Command previousCommand;

        private bool completed;

        public LoggingPageViewModel()
        {
            logger = new LoggerConfiguration()
                .WriteTo.Console(LogEventLevel.Information)
                .CreateLogger();
            Console.SetOut(new ListWriter(log));
            cancelCommand = new Command(_ =>
            {
                logger.Information("Requesting cancellation");
                cancellationTokenSource.Cancel();
                cancelCommand.RaiseCanExecuteChanged();
            }, _ => !cancellationTokenSource.IsCancellationRequested && !completed);
            log.CollectionChanged += (sender, e) => OnPropertyChanged("Log");
        }

        public LoggingPageViewModel(Config config, Page owner) : this()
        {
            this.config = config;
            previousCommand = new Command(_ => owner.NavigationService.GoBack(), _ => completed);
        }

        public ICommand CancelCommand => cancelCommand;
        public string Log => string.Join('\n', log.TakeLast(256));
        public ICommand PreviousCommand => previousCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        internal async Task Convert()
        {
            if (!Directory.Exists("hashes"))
                Directory.CreateDirectory("hashes");
            IReadOnlyList<RepositoryContent> repositoryContents;
            try
            {
                logger.Information("Downloading latest hashtables");
                repositoryContents =
                    await App.GitHubClient.Repository.Content.GetAllContents("CommunityDragon", "CDTB",
                        "cdragontoolbox");
                foreach (var file in repositoryContents.Where(f =>
                    f.Name == "hashes.binhashes.txt" || f.Name == "hashes.game.txt"))
                {
                    var filePath = $"hashes/{file.Name}";
                    var shaFilePath = $"{filePath}.sha";
                    if (!File.Exists(filePath) || !File.Exists(shaFilePath) ||
                        await File.ReadAllTextAsync(shaFilePath) != file.Sha)
                    {
                        var tempFilePath = $"{filePath}.tmp";
                        await File.WriteAllTextAsync(tempFilePath,
                            await App.HttpClient.GetStringAsync(file.DownloadUrl));
                        File.Move(tempFilePath, filePath);
                        await File.WriteAllTextAsync(shaFilePath, file.Sha);
                    }
                }
            }
            catch (Exception)
            {
                if (File.Exists("hashes/hashes.binhashes.txt") && File.Exists("hashes/hashes.game.txt"))
                {
                    logger.Error("Couldn't update hashtables, using current version");
                }
                else
                {
                    logger.Fatal("Couldn't download hashtables, cancelling!");
                    cancellationTokenSource.Cancel();
                }
            }

            if (!cancellationTokenSource.IsCancellationRequested)
                await Task.Run(async () => await Utils.Convert(config, logger, cancellationTokenSource.Token));
            completed = true;
            cancelCommand.RaiseCanExecuteChanged();
            previousCommand.RaiseCanExecuteChanged();
            cancellationTokenSource.Dispose();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}