using System.Diagnostics;
using LearnOnMe.Core.Models;

namespace LearnOnMe.Core.Pins;

/// <summary>Turns what the user typed or dropped into a pin, and a pin into something Process.Start can open.</summary>
public static class PinTargets
{
    private static readonly string[] AppExtensions = [".exe", ".lnk", ".appref-ms"];

    /// <summary>Adds https:// when a link has no scheme, so "drive.google.com/..." opens in the browser.</summary>
    public static string NormalizeLink(string link)
    {
        var trimmed = link.Trim();
        if (trimmed.Length == 0 || trimmed.Contains("://", StringComparison.Ordinal) ||
            trimmed.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }
        return "https://" + trimmed;
    }

    /// <summary>Strips the quotes Explorer's "Copy as path" adds and expands %VARIABLES%.</summary>
    public static string NormalizePath(string path) =>
        Environment.ExpandEnvironmentVariables(path.Trim().Trim('"'));

    public static string Normalize(PinType type, string target) =>
        type == PinType.Link ? NormalizeLink(target) : NormalizePath(target);

    /// <summary>Type for a dropped path: folders stay folders, programs and shortcuts are apps, the rest are files.</summary>
    public static PinType GuessType(string path, bool isDirectory)
    {
        if (isDirectory)
        {
            return PinType.Folder;
        }
        var extension = Path.GetExtension(path);
        return AppExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ? PinType.App : PinType.File;
    }

    /// <summary>A readable default title: the folder or file name, or the link's host.</summary>
    public static string SuggestTitle(PinType type, string target)
    {
        var normalized = Normalize(type, target);
        if (type == PinType.Link)
        {
            return Uri.TryCreate(normalized, UriKind.Absolute, out var uri) && uri.Host.Length > 0
                ? uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host[4..] : uri.Host
                : normalized;
        }

        // Split on both separators by hand so Windows paths behave the same in tests on any OS.
        var trimmed = normalized.TrimEnd('\\', '/');
        var name = trimmed[(trimmed.LastIndexOfAny(['\\', '/']) + 1)..];
        if (type != PinType.Folder && name.LastIndexOf('.') > 0)
        {
            name = name[..name.LastIndexOf('.')];
        }
        // A drive root such as C:\ has no folder name.
        return name.Length > 0 && !name.EndsWith(':') ? name : normalized;
    }

    /// <summary>False when a folder, file or app path cannot be found. Links are always assumed to exist.</summary>
    public static bool Exists(PinType type, string target)
    {
        var path = NormalizePath(target);
        return type switch
        {
            PinType.Link => true,
            PinType.Folder => Directory.Exists(path),
            _ => File.Exists(path),
        };
    }

    /// <summary>Shell-executes the target, which is what double-clicking it in Explorer would do.</summary>
    public static ProcessStartInfo CreateStartInfo(Pin pin)
    {
        var target = Normalize(pin.Type, pin.Target);
        var info = new ProcessStartInfo(target) { UseShellExecute = true };
        if (pin.Type == PinType.App && Path.IsPathRooted(target))
        {
            // Some tools (PAC Machine Edition, older IDEs) expect to start in their own folder.
            info.WorkingDirectory = Path.GetDirectoryName(target) ?? "";
        }
        return info;
    }

    public static void Open(Pin pin)
    {
        using var _ = Process.Start(CreateStartInfo(pin));
    }
}
