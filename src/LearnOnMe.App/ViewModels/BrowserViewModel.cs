using System.Windows.Input;
using LearnOnMe.Core.Pins;

namespace LearnOnMe.App.ViewModels;

/// <summary>
/// The subject's built-in browser tab, where link pins (Drive, Teams, Moodle) open. The view owns the WebView2
/// and reports back the page it is on; this class only asks it to navigate.
/// </summary>
public sealed class BrowserViewModel : ObservableObject
{
    private string _address = "";
    private string _pageTitle = "";
    private string? _currentUrl;

    public BrowserViewModel(Action<string, string> pinPage, Func<string, bool> isPinned, Action<string> openExternally)
    {
        GoCommand = new RelayCommand(() => Navigate(Address), () => Address.Trim().Length > 0);
        PinPageCommand = new RelayCommand(() => pinPage(CurrentUrl!, PageTitle),
            () => CurrentUrl is not null && !isPinned(CurrentUrl));
        OpenExternallyCommand = new RelayCommand(() => openExternally(CurrentUrl!), () => CurrentUrl is not null);
    }

    /// <summary>Raised with the URL to load; the view navigates its WebView2.</summary>
    public event Action<string>? NavigationRequested;

    /// <summary>The last URL asked for, loaded by the view once WebView2 has started.</summary>
    public string? PendingUrl { get; private set; }

    /// <summary>Text in the address bar.</summary>
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string PageTitle
    {
        get => _pageTitle;
        private set => SetProperty(ref _pageTitle, value);
    }

    public string? CurrentUrl
    {
        get => _currentUrl;
        private set
        {
            if (SetProperty(ref _currentUrl, value))
            {
                Raise(nameof(HasPage));
            }
        }
    }

    public bool HasPage => CurrentUrl is not null || PendingUrl is not null;

    public ICommand GoCommand { get; }

    /// <summary>Adds the current page to the subject's link pins; disabled once it is pinned.</summary>
    public ICommand PinPageCommand { get; }

    /// <summary>Opens the current page in the default browser, e.g. when Google refuses to sign in inside the app.</summary>
    public ICommand OpenExternallyCommand { get; }

    public void Navigate(string url)
    {
        var link = PinTargets.NormalizeLink(url);
        PendingUrl = link;
        Address = link;
        Raise(nameof(HasPage));
        NavigationRequested?.Invoke(link);
    }

    /// <summary>Called by the view after each navigation.</summary>
    public void OnNavigated(string url, string title)
    {
        CurrentUrl = url;
        Address = url;
        PageTitle = title;
    }
}
