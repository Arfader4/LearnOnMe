using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace LearnOnMe.App.Services;

public static class DarkTitleBar
{
    private const int DwmwaUseImmersiveDarkMode = 20;

    /// <summary>Windows 10 20H1+ / 11: dark title bar to match the theme. Silently ignored on older systems.</summary>
    public static void Apply(Window window) =>
        window.SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var enabled = 1;
            _ = DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref enabled, sizeof(int));
        };

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
