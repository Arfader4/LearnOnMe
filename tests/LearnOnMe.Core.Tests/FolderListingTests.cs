using LearnOnMe.Core.Study;

namespace LearnOnMe.Core.Tests;

public sealed class FolderListingTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "learnonme-tests", Guid.NewGuid().ToString("N"));

    public FolderListingTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void List_PutsFoldersFirstAndHidesDotAndLockFiles()
    {
        File.WriteAllText(Path.Combine(_root, "wyklad.pdf"), "abc");
        File.WriteAllText(Path.Combine(_root, "Ankieta.docx"), "");
        File.WriteAllText(Path.Combine(_root, "~$Ankieta.docx"), "");
        File.WriteAllText(Path.Combine(_root, ".DS_Store"), "");
        Directory.CreateDirectory(Path.Combine(_root, "Laborki"));
        Directory.CreateDirectory(Path.Combine(_root, "egzamin"));

        var entries = FolderListing.List(_root);

        Assert.Equal(["egzamin", "Laborki", "Ankieta.docx", "wyklad.pdf"], entries.Select(e => e.Name));
        Assert.True(entries[0].IsDirectory);
        Assert.Equal(3, entries[3].Size);
    }

    [Fact]
    public void List_MissingFolderThrows() =>
        Assert.Throws<DirectoryNotFoundException>(() => FolderListing.List(Path.Combine(_root, "brak")));

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1500, "2 KB")]
    [InlineData(3_565_158, "3,4 MB")]
    [InlineData(5L * 1024 * 1024, "5 MB")]
    [InlineData(1_288_490_189, "1,2 GB")]
    public void FormatSize_LooksLikeExplorer(long bytes, string expected) =>
        Assert.Equal(expected, FolderListing.FormatSize(bytes));

    [Fact]
    public void IsBelow_OnlyForRealSubfolders()
    {
        var sub = Path.Combine(_root, "Laborki", "lab1");

        Assert.True(FolderListing.IsBelow(sub, _root));
        Assert.False(FolderListing.IsBelow(_root, _root));
        Assert.False(FolderListing.IsBelow(_root + "-inny", _root));
        Assert.False(FolderListing.IsBelow(Path.GetDirectoryName(_root)!, _root));
    }
}
