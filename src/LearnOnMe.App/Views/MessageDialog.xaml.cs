using System.Windows;
using LearnOnMe.App.Services;

namespace LearnOnMe.App.Views;

/// <summary>Dark replacement for MessageBox: a message plus a named confirm button.</summary>
public partial class MessageDialog : Window
{
    private MessageDialog()
    {
        InitializeComponent();
        DarkTitleBar.Apply(this);
    }

    public static bool Show(Window? owner, string title, string message, string confirmLabel,
        bool isDanger, bool showCancel = true)
    {
        var dialog = new MessageDialog { Owner = owner, Title = title };
        dialog.MessageText.Text = message;
        dialog.ConfirmButton.Content = confirmLabel;
        if (isDanger)
        {
            dialog.ConfirmButton.Style = (Style)dialog.FindResource("DangerButton");
            // Destructive actions should not run on a stray Enter.
            dialog.ConfirmButton.IsDefault = false;
            dialog.Loaded += (_, _) => dialog.CancelButton.Focus();
        }
        else
        {
            dialog.Loaded += (_, _) => dialog.ConfirmButton.Focus();
        }
        if (!showCancel)
        {
            dialog.CancelButton.Visibility = Visibility.Collapsed;
            dialog.ConfirmButton.IsCancel = true;
        }
        return dialog.ShowDialog() == true;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
