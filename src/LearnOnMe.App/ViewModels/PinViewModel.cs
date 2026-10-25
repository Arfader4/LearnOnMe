using LearnOnMe.Core.Models;
using LearnOnMe.Core.Pins;
using LearnOnMe.Core.Study;

namespace LearnOnMe.App.ViewModels;

/// <summary>One card on the subject page.</summary>
public sealed class PinViewModel(Pin pin)
{
    public Pin Pin { get; } = pin;

    public string Title => Pin.Title;

    public string Target => Pin.Target;

    public PinType Type => Pin.Type;

    public int SortOrder => Pin.SortOrder;

    /// <summary>Section header; pins are grouped by this on the card.</summary>
    public string GroupLabel => Pin.Type switch
    {
        PinType.Folder => "Foldery",
        PinType.File => "Pliki",
        PinType.App => "Aplikacje",
        _ => "Linki",
    };

    public string Glyph => Pin.Type switch
    {
        PinType.Folder => "",
        PinType.File => "",
        PinType.App => "",
        _ => "",
    };

    /// <summary>Context-menu label for opening outside the app, or null when the pin already opens in Windows.</summary>
    public string? OpenExternallyLabel => Pin.Type switch
    {
        PinType.Folder => "Otwórz w Eksploratorze",
        PinType.Link when PinTargets.IsWebLink(Pin.Target) => "Otwórz w przeglądarce systemowej",
        PinType.File when StudyFiles.IsHtml(Pin.Target) => "Otwórz w domyślnej przeglądarce",
        _ => null,
    };

    public bool HasExternalOpen => OpenExternallyLabel is not null;

    /// <summary>Checked once when the card loads, so a moved folder shows a warning instead of failing silently.</summary>
    public bool IsMissing { get; } = !PinTargets.Exists(pin.Type, pin.Target);

    public string ToolTipText => IsMissing ? $"{Pin.Target}\nNie znaleziono na dysku." : Pin.Target;
}
