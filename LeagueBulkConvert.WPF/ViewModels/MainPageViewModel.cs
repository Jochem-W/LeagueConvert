using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using LeagueBulkConvert.WPF.Views;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace LeagueBulkConvert.WPF.ViewModels;

internal class MainPageViewModel : INotifyPropertyChanged
{
    private readonly Command _nextCommand;

    private string _exportPath;

    private string _leaguePath;

    private string _wadsPath;

    public MainPageViewModel()
    {
        BrowseExport = new Command(_ => ExportPath = Browse(ExportPath));
        BrowseLeague = new Command(_ => LeaguePath = Browse(LeaguePath));
    }

    public MainPageViewModel(Page owner) : this()
    {
        _nextCommand = new Command(_ =>
        {
            Directory.CreateDirectory(ExportPath);
            Directory.SetCurrentDirectory(ExportPath);
            var config = new Config();
            var t = Directory.EnumerateFiles(_wadsPath, "*.wad.client");
            foreach (var filePath in Directory
                         .EnumerateFiles(_wadsPath, "*.wad.client", SearchOption.AllDirectories)
                         .Where(f => Path.GetFileName(f).Count(c => c == '.') == 2))
                config.Wads.Add(new IncludableWad(filePath));
            owner.NavigationService.Navigate(new ConfigPage(config));
        }, _ => LeaguePath != null && ExportPath != null);
    }

    public ICommand BrowseExport { get; }

    public ICommand BrowseLeague { get; }

    public string ExportPath
    {
        get => _exportPath;
        set
        {
            try
            {
                _exportPath = Path.GetFullPath(value);
            }
            catch (Exception)
            {
                _exportPath = null;
            }

            OnPropertyChanged();
            _nextCommand.RaiseCanExecuteChanged();
        }
    }

    public string LeaguePath
    {
        get => _leaguePath;
        set
        {
            var error = false;
            try
            {
                _leaguePath = Path.GetFullPath(value);
                if (TryGetWadsPath(_leaguePath, out var wadsPath))
                {
                    _wadsPath = wadsPath;
                }
                else
                {
                    _leaguePath = null;
                    _wadsPath = null;
                }
            }
            catch (Exception)
            {
                _leaguePath = null;
                _wadsPath = null;
                error = true;
            }

            if ((_leaguePath == null || _wadsPath == null) && !error)
                new MessageWindow("Invalid directory",
                        "Please select a valid League of Legends installation directory! (e.g. C:\\Riot Games\\League of Legends)")
                    .ShowDialog();
            OnPropertyChanged();
            _nextCommand.RaiseCanExecuteChanged();
        }
    }

    public ICommand NextCommand => _nextCommand;

    public event PropertyChangedEventHandler PropertyChanged;

    private static string Browse(string initialDirectory)
    {
        using var dialog = new CommonOpenFileDialog
        {
            InitialDirectory = initialDirectory,
            IsFolderPicker = true
        };
        return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
    }

    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private static bool TryGetWadsPath(string path, out string wadsPath)
    {
        wadsPath = Path.Combine(path, "Game", "DATA", "FINAL");
        if (Directory.Exists(wadsPath))
            return true;
        var downloadPath = Path.Combine(path, "lol_game_client", "releases");
        var solutionPath = Path.Combine(path, "RADS", "projects", "lol_game_client", "releases");
        string pathPart;
        if (Directory.Exists(downloadPath))
        {
            pathPart = "files";
            wadsPath = downloadPath;
        }
        else if (Directory.Exists(solutionPath))
        {
            pathPart = "deploy";
            wadsPath = solutionPath;
        }
        else
        {
            return false;
        }

        IList<string> directories = Directory.GetDirectories(wadsPath);
        if (directories.Count == 0)
            return false;
        var newestVersion = new Version(Path.GetFileName(directories[0]));
        foreach (var versionString in directories.Skip(1))
        {
            var version = new Version(versionString);
            if (newestVersion.CompareTo(version) < 0)
                newestVersion = version;
        }

        wadsPath = Path.Combine(wadsPath, newestVersion.ToString(), pathPart, "DATA", "FINAL");
        return true;
    }
}