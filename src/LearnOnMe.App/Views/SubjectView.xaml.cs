using System.Windows.Controls;

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
}
