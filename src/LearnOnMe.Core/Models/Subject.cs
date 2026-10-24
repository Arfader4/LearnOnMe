namespace LearnOnMe.Core.Models;

/// <summary>A university subject shown as a tile on the home screen.</summary>
public sealed class Subject
{
    public long Id { get; set; }

    /// <summary>Short label shown in the tile badge, e.g. "BD2".</summary>
    public required string Code { get; set; }

    public required string Name { get; set; }

    /// <summary>Accent colour of the tile as #RRGGBB.</summary>
    public string AccentColor { get; set; } = "#4F8CFF";

    public int SortOrder { get; set; }
}
