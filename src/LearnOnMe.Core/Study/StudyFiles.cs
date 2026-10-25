using System.Security.Cryptography;
using System.Text;
using LearnOnMe.Core.Models;
using LearnOnMe.Core.Pins;

namespace LearnOnMe.Core.Study;

/// <summary>An HTML quiz or flashcard set shown in the "Nauka" tab.</summary>
/// <param name="Root">Folder served to WebView2, so the page's relative scripts, styles and images load.</param>
/// <param name="RelativePath">Path of the file inside <paramref name="Root"/>, with forward slashes.</param>
/// <param name="Group">Title of the pin the file came from; files are grouped by it.</param>
public sealed record StudyFile(string Title, string FullPath, string Root, string RelativePath, string Group)
{
    /// <summary>Subfolder inside the pinned folder, or empty when the file sits at its top.</summary>
    public string Detail => RelativePath.Contains('/') ? RelativePath[..RelativePath.LastIndexOf('/')] : "";
}

/// <summary>Finds the user's own HTML study files in a subject's pins.</summary>
public static class StudyFiles
{
    public const int MaxDepth = 4;
    public const int MaxFiles = 500;

    private static readonly string[] HtmlExtensions = [".html", ".htm"];

    // Build output and tooling folders that can hold thousands of unrelated HTML files.
    private static readonly HashSet<string> SkippedFolders =
        new(["node_modules", "bin", "obj", "__pycache__", "venv"], StringComparer.OrdinalIgnoreCase);

    public static bool IsHtml(string path) =>
        HtmlExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    /// <summary>HTML file pins plus every HTML file inside pinned folders (up to <see cref="MaxDepth"/> levels down).</summary>
    public static IReadOnlyList<StudyFile> Find(IEnumerable<Pin> pins)
    {
        var found = new List<StudyFile>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pin in pins)
        {
            var path = PinTargets.NormalizePath(pin.Target);
            if (pin.Type == PinType.File && IsHtml(path) && File.Exists(path))
            {
                var file = FromFile(path, pin.Title);
                if (seen.Add(file.FullPath))
                {
                    found.Add(file);
                }
            }
            else if (pin.Type == PinType.Folder && Directory.Exists(path))
            {
                var root = Path.GetFullPath(path);
                foreach (var file in Walk(root, root, 0).Order(StringComparer.CurrentCultureIgnoreCase))
                {
                    if (found.Count >= MaxFiles)
                    {
                        return found;
                    }
                    if (seen.Add(file))
                    {
                        found.Add(new StudyFile(Path.GetFileNameWithoutExtension(file), file, root,
                            Path.GetRelativePath(root, file).Replace('\\', '/'), pin.Title));
                    }
                }
            }
        }
        return found;
    }

    /// <summary>A single HTML file, served from its own folder.</summary>
    public static StudyFile FromFile(string path, string group)
    {
        var full = Path.GetFullPath(path);
        return new StudyFile(Path.GetFileNameWithoutExtension(full), full,
            Path.GetDirectoryName(full) ?? full, Path.GetFileName(full), group);
    }

    /// <summary>
    /// Host name WebView2 maps to <paramref name="root"/>. It is derived from the path, so a quiz keeps the same
    /// origin between runs and whatever it saves in localStorage (scores, progress) is still there next time.
    /// </summary>
    public static string VirtualHost(string root)
    {
        var key = Path.GetFullPath(root).TrimEnd('\\', '/').ToUpperInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return $"f{Convert.ToHexStringLower(hash)[..12]}.nauka.learnonme";
    }

    public static Uri VirtualUrl(StudyFile file) =>
        new($"https://{VirtualHost(file.Root)}/" +
            string.Join('/', file.RelativePath.Split('/').Select(Uri.EscapeDataString)));

    private static IEnumerable<string> Walk(string root, string folder, int depth)
    {
        string[] files;
        string[] folders;
        try
        {
            files = Directory.GetFiles(folder);
            folders = depth < MaxDepth ? Directory.GetDirectories(folder) : [];
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            yield break;
        }

        foreach (var file in files.Where(f => IsHtml(f) && !FolderListing.IsHidden(f)))
        {
            yield return file;
        }
        foreach (var sub in folders)
        {
            var name = Path.GetFileName(sub);
            if (SkippedFolders.Contains(name) || FolderListing.IsHidden(sub))
            {
                continue;
            }
            foreach (var file in Walk(root, sub, depth + 1))
            {
                yield return file;
            }
        }
    }
}
