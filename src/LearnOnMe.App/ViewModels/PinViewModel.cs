using LearnOnMe.Core.Models;
using LearnOnMe.Core.Pins;

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

    /// <summary>Checked once when the card loads, so a moved folder shows a warning instead of failing silently.</summary>
    public bool IsMissing { get; } = !PinTargets.Exists(pin.Type, pin.Target);

    public string ToolTipText => IsMissing ? $"{Pin.Target}\nNie znaleziono na dysku." : Pin.Target;
}
