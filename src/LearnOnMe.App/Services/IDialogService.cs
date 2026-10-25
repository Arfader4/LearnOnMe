using LearnOnMe.Core.Models;

namespace LearnOnMe.App.Services;

/// <summary>Windows the view models ask for, kept behind an interface so they stay free of WPF windows.</summary>
public interface IDialogService
{
    /// <summary>Shows the pin editor. Returns the edited pin, or null when the user cancels.</summary>
    Pin? EditPin(Pin pin, bool isNew);

    /// <summary>Asks before a destructive step. <paramref name="confirmLabel"/> names the action, e.g. "Usuń".</summary>
    bool Confirm(string title, string message, string confirmLabel);

    void ShowError(string title, string message);
}
