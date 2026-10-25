using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using LearnOnMe.App.Services;
using LearnOnMe.Core.Abstractions;
using LearnOnMe.Core.Models;
using LearnOnMe.Core.Pins;
using LearnOnMe.Core.Study;

namespace LearnOnMe.App.ViewModels;

public enum SubjectTab
{
    Pins,
    Study,
    Browser,
}

/// <summary>
/// Subject card with three tabs: pins (with a preview of pinned folders), "Nauka" with the user's own HTML
/// quizzes, and a built-in browser for link pins.
/// </summary>
public sealed class SubjectViewModel : ObservableObject
{
    private readonly IPinRepository _pins;
    private readonly IDialogService _dialogs;
    private SubjectTab _tab;
    private FolderPreviewViewModel? _folder;

    public SubjectViewModel(SubjectTileViewModel tile, IPinRepository pins, IDialogService dialogs, Action goHome)
    {
        Tile = tile;
        _pins = pins;
        _dialogs = dialogs;
        BackCommand = new RelayCommand(goHome);
        AddPinCommand = new RelayCommand(p => AddPin(p as PinType? ?? PinType.Folder));
        OpenPinCommand = new RelayCommand(p => OpenPin((PinViewModel)p!), p => p is PinViewModel);
        EditPinCommand = new RelayCommand(p => EditPin((PinViewModel)p!), p => p is PinViewModel);
        DeletePinCommand = new RelayCommand(p => DeletePin((PinViewModel)p!), p => p is PinViewModel);
        OpenPinExternallyCommand = new RelayCommand(p => OpenExternally((PinViewModel)p!), p => p is PinViewModel);
        ShowTabCommand = new RelayCommand(p => Tab = (SubjectTab)p!, p => p is SubjectTab);
        Browser = new BrowserViewModel(PinPage, IsPinned, ShellOpen);
        Study = new StudyViewModel(() => Pins.Select(p => p.Pin), () => AddPin(PinType.File), ShellOpen, OpenInBrowser);
        Pins.CollectionChanged += (_, _) =>
        {
            Raise(nameof(HasPins), nameof(ShowPinGrid));
            Study.MarkStale();
        };
    }

    public StudyViewModel Study { get; }

    public BrowserViewModel Browser { get; }

    public SubjectTab Tab
    {
        get => _tab;
        set
        {
            if (SetProperty(ref _tab, value))
            {
                Raise(nameof(IsPinsTab), nameof(IsStudyTab), nameof(IsBrowserTab));
                if (value == SubjectTab.Study)
                {
                    _ = Study.EnsureLoadedAsync();
                }
            }
        }
    }

    // The tab radio buttons bind to these; only the checked one changes the tab.
    public bool IsPinsTab
    {
        get => Tab == SubjectTab.Pins;
        set { if (value) Tab = SubjectTab.Pins; }
    }

    public bool IsStudyTab
    {
        get => Tab == SubjectTab.Study;
        set { if (value) Tab = SubjectTab.Study; }
    }

    public bool IsBrowserTab
    {
        get => Tab == SubjectTab.Browser;
        set { if (value) Tab = SubjectTab.Browser; }
    }

    /// <summary>The pinned folder being previewed in the pins tab, or null when the pin grid is shown.</summary>
    public FolderPreviewViewModel? Folder
    {
        get => _folder;
        private set
        {
            if (SetProperty(ref _folder, value))
            {
                Raise(nameof(ShowPinGrid));
            }
        }
    }

    public bool ShowPinGrid => HasPins && Folder is null;

    public SubjectTileViewModel Tile { get; }

    public ObservableCollection<PinViewModel> Pins { get; } = [];

    public bool HasPins => Pins.Count > 0;

    public ICommand BackCommand { get; }

    /// <summary>Parameter: the <see cref="PinType"/> preselected in the editor.</summary>
    public ICommand AddPinCommand { get; }

    public ICommand OpenPinCommand { get; }

    public ICommand EditPinCommand { get; }

    public ICommand DeletePinCommand { get; }

    /// <summary>Folder in Explorer, link in the default browser, file in its default program.</summary>
    public ICommand OpenPinExternallyCommand { get; }

    /// <summary>Parameter: the <see cref="SubjectTab"/> to show.</summary>
    public ICommand ShowTabCommand { get; }

    public void Load()
    {
        Pins.Clear();
        foreach (var pin in _pins.GetForSubject(Tile.Subject.Id))
        {
            Pins.Add(new PinViewModel(pin));
        }
    }

    /// <summary>Files and folders dragged from Explorer, or a link dragged from the browser, become pins straight away.</summary>
    public void AddDropped(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var type = PinTargets.GuessType(path, Directory.Exists(path));
            Save(NewPin(type, path, PinTargets.SuggestTitle(type, path)));
        }
    }

    public void AddDroppedLink(string url)
    {
        var link = PinTargets.NormalizeLink(url);
        Save(NewPin(PinType.Link, link, PinTargets.SuggestTitle(PinType.Link, link)));
    }

    private void PinPage(string url, string pageTitle)
    {
        var title = pageTitle.Trim().Length > 0 ? pageTitle.Trim() : PinTargets.SuggestTitle(PinType.Link, url);
        Save(NewPin(PinType.Link, url, title.Length > 80 ? title[..80] : title));
    }

    private bool IsPinned(string url) =>
        Pins.Any(p => p.Type == PinType.Link &&
                      string.Equals(PinTargets.NormalizeLink(p.Target).TrimEnd('/'), url.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));

    private void AddPin(PinType type)
    {
        var edited = _dialogs.EditPin(NewPin(type, "", ""), isNew: true);
        if (edited is not null)
        {
            Save(edited);
        }
    }

    private void EditPin(PinViewModel item)
    {
        var edited = _dialogs.EditPin(item.Pin, isNew: false);
        if (edited is not null)
        {
            _pins.Update(edited);
            Pins[Pins.IndexOf(item)] = new PinViewModel(edited);
        }
    }

    private void DeletePin(PinViewModel item)
    {
        if (_dialogs.Confirm("Usuń przypinkę",
                item.Type == PinType.Link
                    ? $"Usunąć „{item.Title}” z karty {Tile.Name}?"
                    : $"Usunąć „{item.Title}” z karty {Tile.Name}? Sam {Describe(item.Type)} zostanie na dysku.",
                "Usuń przypinkę"))
        {
            _pins.Delete(item.Pin.Id);
            Pins.Remove(item);
        }
    }

    /// <summary>Folders open in the preview, links in the browser tab, HTML files in "Nauka", the rest in Windows.</summary>
    private void OpenPin(PinViewModel item)
    {
        if (item.IsMissing && item.Type != PinType.Link)
        {
            ShowOpenError(item.Title, item.Target, item.Type);
            return;
        }
        switch (item.Type)
        {
            case PinType.Folder:
                Folder = new FolderPreviewViewModel(item.Title, PinTargets.NormalizePath(item.Target),
                    OpenFolderEntry, ShellOpen, () => Folder = null);
                break;
            case PinType.Link when PinTargets.IsWebLink(item.Target):
                OpenInBrowser(item.Target);
                break;
            case PinType.File when StudyFiles.IsHtml(item.Target):
                OpenInStudy(PinTargets.NormalizePath(item.Target), item.Title);
                break;
            default:
                OpenExternally(item);
                break;
        }
    }

    private void OpenExternally(PinViewModel item)
    {
        try
        {
            PinTargets.Open(item.Pin);
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            ShowOpenError(item.Title, item.Target, item.Type);
        }
    }

    private void OpenFolderEntry(FolderEntry entry)
    {
        if (StudyFiles.IsHtml(entry.Name))
        {
            OpenInStudy(entry.FullPath, Folder?.Title ?? Tile.Name);
        }
        else
        {
            ShellOpen(entry.FullPath);
        }
    }

    private void OpenInBrowser(string url)
    {
        if (PinTargets.IsWebLink(url))
        {
            Browser.Navigate(url);
            Tab = SubjectTab.Browser;
        }
        else
        {
            ShellOpen(url);
        }
    }

    private void OpenInStudy(string path, string group)
    {
        Tab = SubjectTab.Study;
        _ = Study.OpenAsync(path, group);
    }

    /// <summary>Opens a path or URL outside the app; used by the folder preview, the browser and "Nauka".</summary>
    private void ShellOpen(string target)
    {
        try
        {
            PinTargets.ShellOpen(target);
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            _dialogs.ShowError("Nie udało się otworzyć", $"Windows nie mógł otworzyć:\n{target}");
        }
    }

    private void ShowOpenError(string title, string target, PinType type) =>
        _dialogs.ShowError($"Nie udało się otworzyć „{title}”",
            $"Windows nie mógł otworzyć:\n{target}\n\nSprawdź, czy {Describe(type)} nadal tam jest, " +
            "i popraw ścieżkę przez Edytuj (prawy przycisk na przypince).");

    private Pin NewPin(PinType type, string target, string title) => new()
    {
        SubjectId = Tile.Subject.Id,
        Type = type,
        Target = target,
        Title = title,
        SortOrder = Pins.Count == 0 ? 0 : Pins.Max(p => p.SortOrder) + 1,
    };

    private void Save(Pin pin)
    {
        _pins.Add(pin);
        Pins.Add(new PinViewModel(pin));
    }

    private static string Describe(PinType type) => type switch
    {
        PinType.Folder => "folder",
        PinType.File => "plik",
        PinType.App => "program",
        _ => "link",
    };
}
