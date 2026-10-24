namespace LearnOnMe.Core.Models;

/// <summary>What a pin opens. Stored as an integer, so never renumber.</summary>
public enum PinType
{
    Folder = 0,
    File = 1,
    App = 2,
    Link = 3,
}
