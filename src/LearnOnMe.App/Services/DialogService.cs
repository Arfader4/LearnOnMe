using System.Windows;
using LearnOnMe.App.ViewModels;
using LearnOnMe.App.Views;
using LearnOnMe.Core.Models;

namespace LearnOnMe.App.Services;

public sealed class DialogService : IDialogService
{
    public Pin? EditPin(Pin pin, bool isNew)
    {
        var editor = new PinEditorViewModel(pin, isNew);
        var window = new PinEditorWindow { DataContext = editor, Owner = ActiveWindow() };
        return window.ShowDialog() == true ? editor.Result : null;
    }

    public bool Confirm(string title, string message, string confirmLabel) =>
        MessageDialog.Show(ActiveWindow(), title, message, confirmLabel, isDanger: true);

    public void ShowError(string title, string message) =>
        MessageDialog.Show(ActiveWindow(), title, message, confirmLabel: "Zamknij", isDanger: false, showCancel: false);

    private static Window? ActiveWindow() =>
        Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow;
}
