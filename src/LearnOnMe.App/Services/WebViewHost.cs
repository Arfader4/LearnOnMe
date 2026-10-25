using System.IO;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace LearnOnMe.App.Services;

/// <summary>
/// One WebView2 profile for the whole app, kept in %LOCALAPPDATA%\LearnOnMe\WebView2, so logins to Drive or Teams
/// and what quizzes save in localStorage survive a restart.
/// </summary>
public static class WebViewHost
{
    public const string RuntimeDownloadUrl = "https://developer.microsoft.com/microsoft-edge/webview2/";

    private static Task<CoreWebView2Environment>? _environment;

    /// <summary>Background shown before a page paints, so switching tabs does not flash white.</summary>
    public static readonly System.Drawing.Color Background = System.Drawing.Color.FromArgb(0x12, 0x14, 0x18);

    /// <summary>Returns false when the WebView2 Runtime is not installed (rare on Windows 10/11, but possible).</summary>
    public static async Task<bool> InitializeAsync(WebView2 view)
    {
        view.DefaultBackgroundColor = Background;
        try
        {
            _environment ??= CoreWebView2Environment.CreateAsync(userDataFolder: Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LearnOnMe", "WebView2"));
            await view.EnsureCoreWebView2Async(await _environment);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            _environment = null;
            return false;
        }
        view.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Dark;
        return true;
    }
}
