using System.Collections.ObjectModel;
using System.Windows.Input;
using LearnOnMe.Core.Models;
using LearnOnMe.Core.Study;

namespace LearnOnMe.App.ViewModels;

/// <summary>The "Nauka" tab: the user's own HTML quizzes and flashcards, found in the subject's pins.</summary>
public sealed class StudyViewModel : ObservableObject
{
    private readonly Func<IEnumerable<Pin>> _pins;
    private readonly Action<string> _openLink;
    private StudyFile? _selected;
    private bool _isLoading;
    private bool _isStale = true;

    public StudyViewModel(Func<IEnumerable<Pin>> pins, Action addHtmlFile, Action<string> openExternally,
        Action<string> openLink)
    {
        _pins = pins;
        _openLink = openLink;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
        AddFileCommand = new RelayCommand(addHtmlFile);
        OpenExternallyCommand = new RelayCommand(() => openExternally(Selected!.FullPath), () => Selected is not null);
    }

    public ObservableCollection<StudyFile> Files { get; } = [];

    public bool HasFiles => Files.Count > 0;

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <summary>The file shown in WebView2; the view navigates when this changes.</summary>
    public StudyFile? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public ICommand RefreshCommand { get; }

    public ICommand AddFileCommand { get; }

    public ICommand OpenExternallyCommand { get; }

    /// <summary>A quiz linked to a web page (e.g. the lecture on Drive); it opens in the browser tab, not over the quiz.</summary>
    public void OpenLink(string url) => _openLink(url);

    /// <summary>Pins changed; rescan the next time the tab is shown rather than walking folders on every edit.</summary>
    public void MarkStale() => _isStale = true;

    public async Task EnsureLoadedAsync()
    {
        if (_isStale)
        {
            await RefreshAsync();
        }
    }

    /// <summary>Shows one HTML file, e.g. an HTML file pin or a file picked in the folder preview.</summary>
    public async Task OpenAsync(string path, string group)
    {
        await EnsureLoadedAsync();
        Selected = Files.FirstOrDefault(f => string.Equals(f.FullPath, System.IO.Path.GetFullPath(path),
                       StringComparison.OrdinalIgnoreCase))
                   ?? StudyFiles.FromFile(path, group);
    }

    private async Task RefreshAsync()
    {
        if (IsLoading)
        {
            return;
        }
        IsLoading = true;
        _isStale = false;
        var pins = _pins().ToList();
        // Pinned folders may live on OneDrive or a network drive, so stay off the UI thread.
        var found = await Task.Run(() => StudyFiles.Find(pins));
        // Clearing the list makes the ListBox drop its selection, so remember it first.
        var keep = Selected?.FullPath;
        Files.Clear();
        foreach (var file in found)
        {
            Files.Add(file);
        }
        Raise(nameof(HasFiles));
        IsLoading = false;
        Selected = Files.FirstOrDefault(f => f.FullPath == keep) ?? Files.FirstOrDefault();
    }
}
