using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LearnOnMe.App.Services;
using LearnOnMe.App.ViewModels;
using Microsoft.Web.WebView2.Core;

namespace LearnOnMe.App.Views;

/// <summary>The subject's built-in browser. The view model decides what to open; this drives the WebView2.</summary>
public partial class BrowserView : UserControl
{
    private BrowserViewModel? _vm;
    private bool _ready;
    private bool _disposed;

    public BrowserView()
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
            _vm.NavigationRequested -= Navigate;
        }
        _vm = e.NewValue as BrowserViewModel;
        if (_vm is not null)
        {
            _vm.NavigationRequested += Navigate;
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
        var core = WebView.CoreWebView2;
        core.SourceChanged += (_, _) => Report();
        core.DocumentTitleChanged += (_, _) => Report();
        core.NewWindowRequested += OnNewWindowRequested;
        if (_vm?.PendingUrl is { } url)
        {
            Navigate(url);
        }
    }

    private void Navigate(string url)
    {
        if (!_ready)
        {
            return; // Loaded after initialization from PendingUrl.
        }
        try
        {
            WebView.CoreWebView2.Navigate(url);
        }
        catch (ArgumentException)
        {
            // Not a valid URL; leave the address bar as typed so it can be corrected.
        }
    }

    private void Report()
    {
        if (!AddressBox.IsKeyboardFocused)
        {
            _vm?.OnNavigated(WebView.CoreWebView2.Source, WebView.CoreWebView2.DocumentTitle);
        }
    }

    // target="_blank" links open here; real popups (sign-in windows with a size) keep their own window.
    private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        if (!e.WindowFeatures.HasSize && !e.WindowFeatures.HasPosition)
        {
            e.Handled = true;
            WebView.CoreWebView2.Navigate(e.Uri);
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e) => WebView.GoBack();

    private void Forward_Click(object sender, RoutedEventArgs e) => WebView.GoForward();

    private void Reload_Click(object sender, RoutedEventArgs e) => WebView.Reload();

    private void AddressBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) =>
        Dispatcher.BeginInvoke(AddressBox.SelectAll);
}
