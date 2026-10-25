using System.Windows.Input;
using LearnOnMe.Core.Models;
using LearnOnMe.Core.Pins;

namespace LearnOnMe.App.ViewModels;

/// <summary>Add/edit form for one pin. Works on a copy, so cancelling leaves the original untouched.</summary>
public sealed class PinEditorViewModel : ObservableObject
{
    private readonly Pin _original;
    private PinType _type;
    private string _title;
    private string _target;
    private string? _error;
    private bool _titleEdited;

    public PinEditorViewModel(Pin pin, bool isNew)
    {
        _original = pin;
        _type = pin.Type;
        _title = pin.Title;
        _target = pin.Target;
        // Keep a title the user already chose; only suggest one for new pins.
        _titleEdited = !isNew || pin.Title.Length > 0;
        WindowTitle = isNew ? "Nowa przypinka" : "Edytuj przypinkę";
        SaveLabel = isNew ? "Dodaj przypinkę" : "Zapisz zmiany";
        SaveCommand = new RelayCommand(Save);
    }

    /// <summary>Raised with true after a successful save.</summary>
    public event Action<bool>? CloseRequested;

    public string WindowTitle { get; }

    public string SaveLabel { get; }

    public ICommand SaveCommand { get; }

    public Pin? Result { get; private set; }

    public PinType Type
    {
        get => _type;
        set
        {
            if (SetProperty(ref _type, value))
            {
                Raise(nameof(IsFolder), nameof(IsFile), nameof(IsApp), nameof(IsLink),
                    nameof(TargetLabel), nameof(TargetHint), nameof(CanBrowse), nameof(TargetWarning));
                SuggestTitle();
            }
        }
    }

    // Radio buttons bind to these; only the checked one sets the type.
    public bool IsFolder { get => Type == PinType.Folder; set { if (value) Type = PinType.Folder; } }

    public bool IsFile { get => Type == PinType.File; set { if (value) Type = PinType.File; } }

    public bool IsApp { get => Type == PinType.App; set { if (value) Type = PinType.App; } }

    public bool IsLink { get => Type == PinType.Link; set { if (value) Type = PinType.Link; } }

    public bool CanBrowse => Type != PinType.Link;

    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                _titleEdited = value.Length > 0;
            }
        }
    }

    public string Target
    {
        get => _target;
        set
        {
            if (SetProperty(ref _target, value))
            {
                Error = null;
                Raise(nameof(TargetWarning));
                SuggestTitle();
            }
        }
    }

    public string TargetLabel => Type switch
    {
        PinType.Folder => "Ścieżka folderu",
        PinType.File => "Ścieżka pliku",
        PinType.App => "Program (.exe) albo skrót z menu Start",
        _ => "Adres",
    };

    public string TargetHint => Type switch
    {
        PinType.Folder => "Np. C:\\Studia\\BD2 albo folder OneDrive zsynchronizowany na dysku.",
        PinType.File => "Plik otworzy się w domyślnym programie, np. .pdf w czytniku, .pkt w Packet Tracerze.",
        PinType.App => "Np. SSMS, Packet Tracer, PAC Machine Edition, Visual Studio. Skróty (.lnk) znajdziesz w „Przeglądaj”.",
        _ => "Drive, Teams, OneDrive, UPeL, NetAcad… Brakujące https:// zostanie dodane.",
    };

    /// <summary>Soft warning only: a folder on an unplugged drive is still worth keeping.</summary>
    public string? TargetWarning =>
        Type != PinType.Link && Target.Trim().Length > 0 && !PinTargets.Exists(Type, Target)
            ? "Nie widzę tego na dysku. Przypinka się zapisze, ale może się nie otworzyć."
            : null;

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    private void SuggestTitle()
    {
        if (!_titleEdited && Target.Trim().Length > 0)
        {
            _title = PinTargets.SuggestTitle(Type, Target);
            Raise(nameof(Title));
        }
    }

    private void Save()
    {
        if (Target.Trim().Length == 0)
        {
            Error = Type == PinType.Link ? "Wklej adres, który ma się otwierać." : "Wskaż ścieżkę albo użyj „Przeglądaj”.";
            return;
        }

        var target = PinTargets.Normalize(Type, Target);
        var title = Title.Trim();
        Result = new Pin
        {
            Id = _original.Id,
            SubjectId = _original.SubjectId,
            SortOrder = _original.SortOrder,
            Type = Type,
            Target = target,
            Title = title.Length > 0 ? title : PinTargets.SuggestTitle(Type, target),
        };
        CloseRequested?.Invoke(true);
    }
}
