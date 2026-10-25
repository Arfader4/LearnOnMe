using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using LearnOnMe.Core.Study;

namespace LearnOnMe.App.ViewModels;

/// <summary>A pinned folder shown on the card: browse down into subfolders, double-click a file to open it.</summary>
public sealed class FolderPreviewViewModel : ObservableObject
{
    private readonly string _root;
    private readonly Action<FolderEntry> _openFile;
    private readonly Action<string> _openInExplorer;
    private string _currentPath;
    private string? _error;

    public FolderPreviewViewModel(string title, string root, Action<FolderEntry> openFile,
        Action<string> openInExplorer, Action close)
    {
        Title = title;
        _root = Path.GetFullPath(root);
        _currentPath = _root;
        _openFile = openFile;
        _openInExplorer = openInExplorer;
        OpenCommand = new RelayCommand(p => Open((FolderEntryViewModel)p!), p => p is FolderEntryViewModel);
        UpCommand = new RelayCommand(Up, () => CanGoUp);
        OpenInExplorerCommand = new RelayCommand(() => _openInExplorer(CurrentPath));
        CloseCommand = new RelayCommand(close);
        Load();
    }

    /// <summary>The pin's title.</summary>
    public string Title { get; }

    public string CurrentPath
    {
        get => _currentPath;
        private set
        {
            if (SetProperty(ref _currentPath, value))
            {
                Raise(nameof(Breadcrumb), nameof(CanGoUp));
            }
        }
    }

    /// <summary>"Notatki › Wykład 3": the pin title, then the subfolders below it.</summary>
    public string Breadcrumb
    {
        get
        {
            var relative = Path.GetRelativePath(_root, CurrentPath);
            return relative == "."
                ? Title
                : string.Join(" › ", relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Prepend(Title));
        }
    }

    public bool CanGoUp => FolderListing.IsBelow(CurrentPath, _root);

    public ObservableCollection<FolderEntryViewModel> Entries { get; } = [];

    public bool IsEmpty => Entries.Count == 0 && Error is null;

    /// <summary>Why the folder could not be listed, or null.</summary>
    public string? Error
    {
        get => _error;
        private set
        {
            if (SetProperty(ref _error, value))
            {
                Raise(nameof(IsEmpty));
            }
        }
    }

    public ICommand OpenCommand { get; }

    public ICommand UpCommand { get; }

    public ICommand OpenInExplorerCommand { get; }

    public ICommand CloseCommand { get; }

    private void Open(FolderEntryViewModel entry)
    {
        if (entry.Entry.IsDirectory)
        {
            CurrentPath = entry.Entry.FullPath;
            Load();
        }
        else
        {
            _openFile(entry.Entry);
        }
    }

    private void Up()
    {
        CurrentPath = Path.GetDirectoryName(CurrentPath) ?? _root;
        Load();
    }

    private void Load()
    {
        Entries.Clear();
        try
        {
            foreach (var entry in FolderListing.List(CurrentPath))
            {
                Entries.Add(new FolderEntryViewModel(entry));
            }
            Error = null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Error = ex is UnauthorizedAccessException
                ? "Windows nie pozwala odczytać tego folderu."
                : $"Nie znaleziono folderu:\n{CurrentPath}";
        }
        Raise(nameof(IsEmpty));
    }
}

public sealed class FolderEntryViewModel(FolderEntry entry)
{
    public FolderEntry Entry { get; } = entry;

    public string Name => Entry.Name;

    public string Glyph => Entry.IsDirectory ? "" : StudyFiles.IsHtml(Entry.Name) ? "" : "";

    public bool IsDirectory => Entry.IsDirectory;

    public string SizeText => Entry.IsDirectory ? "" : FolderListing.FormatSize(Entry.Size);

    public string ModifiedText => Entry.Modified.ToString("dd.MM.yyyy HH:mm");

    public string ToolTipText => Entry.IsDirectory
        ? "Dwuklik otwiera folder"
        : StudyFiles.IsHtml(Entry.Name) ? "Dwuklik otwiera w zakładce Nauka" : "Dwuklik otwiera w domyślnym programie";
}
