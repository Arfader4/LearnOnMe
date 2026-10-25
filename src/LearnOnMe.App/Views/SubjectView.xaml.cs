using System.Windows;
using System.Windows.Controls;
using LearnOnMe.App.ViewModels;

namespace LearnOnMe.App.Views;

public partial class SubjectView : UserControl
{
    public SubjectView()
    {
        InitializeComponent();
        // Focus the view so Esc works without clicking first.
        Loaded += (_, _) => Focus();
        Focusable = true;
        FocusVisualStyle = null;
    }

    private void Card_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) || DroppedLink(e) is not null
            ? DragDropEffects.Link
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void Card_Drop(object sender, DragEventArgs e)
    {
        if (DataContext is not SubjectViewModel vm)
        {
            return;
        }
        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
        {
            vm.AddDropped(paths);
        }
        else if (DroppedLink(e) is { } link)
        {
            vm.AddDroppedLink(link);
        }
    }

    // Browsers put the dragged address in the text formats.
    private static string? DroppedLink(DragEventArgs e) =>
        e.Data.GetData(DataFormats.UnicodeText) is string text &&
        Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.AbsoluteUri
            : null;
}
