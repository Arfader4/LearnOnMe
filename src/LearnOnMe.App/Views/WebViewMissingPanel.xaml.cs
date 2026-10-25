using System.Windows;
using System.Windows.Controls;
using LearnOnMe.App.Services;
using LearnOnMe.Core.Pins;

namespace LearnOnMe.App.Views;

public partial class WebViewMissingPanel : UserControl
{
    public WebViewMissingPanel() => InitializeComponent();

    private void Download_Click(object sender, RoutedEventArgs e) => PinTargets.ShellOpen(WebViewHost.RuntimeDownloadUrl);
}
