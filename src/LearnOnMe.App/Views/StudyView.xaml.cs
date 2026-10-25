using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LearnOnMe.App.Services;
using LearnOnMe.App.ViewModels;
using LearnOnMe.Core.Study;
using Microsoft.Web.WebView2.Core;

namespace LearnOnMe.App.Views;

/// <summary>
/// Shows the selected HTML file in WebView2. Each pinned folder is served under its own https host instead of
/// file://, so the quiz's scripts can fetch JSON next to it and its localStorage survives restarts.
/// </summary>
public partial class StudyView : UserControl
{
    private const string HostSuffix = ".nauka.learnonme";

    private readonly HashSet<string> _mappedHosts = new(StringComparer.OrdinalIgnoreCase);
    private StudyViewModel? _vm;
    private bool _ready;
    private bool _disposed;

    public StudyView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += async (_, _) => await InitializeAsync();
        Unloaded += (_, _) =>
        {
            _disposed = true;
            WebView.Dispose();
        };
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
        {
            _vm.PropertyChanged -= OnViewModelChanged;
        }
        _vm = e.NewValue as StudyViewModel;
        if (_vm is not null)
        {
            _vm.PropertyChanged += OnViewModelChanged;
        }
    }

    private async Task InitializeAsync()
    {
        if (_ready || _disposed)
        {
            return;
        }
        if (!await WebViewHost.InitializeAsync(WebView))
        {
            WebView.Visibility = Visibility.Collapsed;
            RuntimeMissing.Visibility = Visibility.Visible;
            return;
        }
        if (_disposed)
        {
            return;
        }
        _ready = true;
        WebView.CoreWebView2.NavigationStarting += OnNavigationStarting;
        WebView.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        Show(_vm?.Selected);
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StudyViewModel.Selected))
        {
            Show(_vm?.Selected);
        }
    }

    private void Show(StudyFile? file)
    {
        if (!_ready || file is null)
        {
            return;
        }
        var host = StudyFiles.VirtualHost(file.Root);
        if (_mappedHosts.Add(host))
        {
            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(host, file.Root, CoreWebView2HostResourceAccessKind.Allow);
        }
        WebView.CoreWebView2.Navigate(StudyFiles.VirtualUrl(file).AbsoluteUri);
    }

    // Links out of the quiz go to the browser tab so the quiz stays where it was.
    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (!IsOwnPage(e.Uri))
        {
            e.Cancel = true;
            _vm?.OpenLink(e.Uri);
        }
    }

    private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        if (IsOwnPage(e.Uri))
        {
            WebView.CoreWebView2.Navigate(e.Uri);
        }
        else
        {
            _vm?.OpenLink(e.Uri);
        }
    }

    private static bool IsOwnPage(string uri) =>
        !Uri.TryCreate(uri, UriKind.Absolute, out var parsed) ||
        parsed.Host.EndsWith(HostSuffix, StringComparison.OrdinalIgnoreCase) ||
        parsed.Scheme is "about" or "data" or "blob" or "javascript";
}
