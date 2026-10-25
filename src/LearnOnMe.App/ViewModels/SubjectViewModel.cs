using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using LearnOnMe.App.Services;
using LearnOnMe.Core.Abstractions;
using LearnOnMe.Core.Models;
using LearnOnMe.Core.Pins;

namespace LearnOnMe.App.ViewModels;

/// <summary>Subject card: pins to folders, files, apps and links. The "Nauka" tab arrives in the next stage.</summary>
public sealed class SubjectViewModel : ObservableObject
{
    private readonly IPinRepository _pins;
    private readonly IDialogService _dialogs;

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
        Pins.CollectionChanged += (_, _) => Raise(nameof(HasPins));
    }

    public SubjectTileViewModel Tile { get; }

    public ObservableCollection<PinViewModel> Pins { get; } = [];

    public bool HasPins => Pins.Count > 0;

    public ICommand BackCommand { get; }

    /// <summary>Parameter: the <see cref="PinType"/> preselected in the editor.</summary>
    public ICommand AddPinCommand { get; }

    public ICommand OpenPinCommand { get; }

    public ICommand EditPinCommand { get; }

    public ICommand DeletePinCommand { get; }

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

    private void OpenPin(PinViewModel item)
    {
        try
        {
            PinTargets.Open(item.Pin);
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            _dialogs.ShowError($"Nie udało się otworzyć „{item.Title}”",
                $"Windows nie mógł otworzyć:\n{item.Target}\n\nSprawdź, czy {Describe(item.Type)} nadal tam jest, " +
                "i popraw ścieżkę przez Edytuj (prawy przycisk na przypince).");
        }
    }

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
