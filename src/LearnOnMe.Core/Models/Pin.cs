namespace LearnOnMe.Core.Models;

/// <summary>A shortcut on a subject card: a folder, file, app or link.</summary>
public sealed class Pin
{
    public long Id { get; set; }

    public long SubjectId { get; set; }

    public PinType Type { get; set; }

    public required string Title { get; set; }

    /// <summary>Path or URL passed to Process.Start.</summary>
    public required string Target { get; set; }

    public int SortOrder { get; set; }
}
