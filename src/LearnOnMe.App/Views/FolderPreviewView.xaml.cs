using System.Windows.Controls;
using System.Windows.Input;
using LearnOnMe.App.ViewModels;

namespace LearnOnMe.App.Views;

public partial class FolderPreviewView : UserControl
{
    public FolderPreviewView() => InitializeComponent();

    private void Entry_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is FolderPreviewViewModel vm && sender is ListBoxItem { DataContext: FolderEntryViewModel entry })
        {
            vm.OpenCommand.Execute(entry);
            e.Handled = true;
        }
    }
}
