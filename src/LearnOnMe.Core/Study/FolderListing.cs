using System.Globalization;

namespace LearnOnMe.Core.Study;

/// <summary>One row in the folder preview.</summary>
public sealed record FolderEntry(string Name, string FullPath, bool IsDirectory, long Size, DateTime Modified);

/// <summary>Reads a folder for the preview on the subject card. Not an explorer: just enough to find a file and open it.</summary>
public static class FolderListing
{
    private static readonly NumberFormatInfo PolishNumbers = new() { NumberDecimalSeparator = "," };

    /// <summary>Subfolders first, then files, both by name. Hidden and system entries are left out, as Explorer does by default.</summary>
    /// <exception cref="DirectoryNotFoundException">The folder was moved or deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">Windows refused to list it.</exception>
    public static IReadOnlyList<FolderEntry> List(string folder)
    {
        var info = new DirectoryInfo(folder);
        var entries = new List<FolderEntry>();
        foreach (var item in info.EnumerateFileSystemInfos())
        {
            if (IsHidden(item))
            {
                continue;
            }
            entries.Add(item is FileInfo file
                ? new FolderEntry(file.Name, file.FullName, false, file.Length, file.LastWriteTime)
                : new FolderEntry(item.Name, item.FullName, true, 0, item.LastWriteTime));
        }
        return entries
            .OrderByDescending(e => e.IsDirectory)
            .ThenBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public static bool IsHidden(string path)
    {
        var name = Path.GetFileName(path.TrimEnd('\\', '/'));
        if (IsHiddenName(name))
        {
            return true;
        }
        try
        {
            return (File.GetAttributes(path) & (FileAttributes.Hidden | FileAttributes.System)) != 0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>"0 B", "12 KB", "3,4 MB", "1,2 GB"; the way Explorer shows sizes, with a Polish decimal comma.</summary>
    public static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{Math.Ceiling(bytes / 1024.0):0} KB",
        < 1024L * 1024 * 1024 => (bytes / (1024.0 * 1024)).ToString("0.#", PolishNumbers) + " MB",
        _ => (bytes / (1024.0 * 1024 * 1024)).ToString("0.#", PolishNumbers) + " GB",
    };

    /// <summary>True when <paramref name="path"/> lies below <paramref name="root"/>, so the preview can go up a level.</summary>
    public static bool IsBelow(string path, string root)
    {
        var p = Path.GetFullPath(path).TrimEnd('\\', '/');
        var r = Path.GetFullPath(root).TrimEnd('\\', '/');
        return p.Length > r.Length + 1 && p.StartsWith(r, StringComparison.OrdinalIgnoreCase) &&
               p[r.Length] is '\\' or '/';
    }

    private static bool IsHidden(FileSystemInfo item) =>
        IsHiddenName(item.Name) || (item.Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0;

    // Dotfiles and Office lock files such as ~$wyklad.docx.
    private static bool IsHiddenName(string name) =>
        name.StartsWith('.') || name.StartsWith("~$", StringComparison.Ordinal);
}
