using LearnOnMe.Core.Models;
using LearnOnMe.Core.Study;

namespace LearnOnMe.Core.Tests;

public sealed class StudyFilesTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "learnonme-tests", Guid.NewGuid().ToString("N"));

    public StudyFilesTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Find_CollectsHtmlFromPinnedFoldersAndFiles()
    {
        var quizzes = Touch("BD2", "quiz-sql.html");
        Touch("BD2", "wyklad.pdf");
        Touch("BD2", "fiszki", "normalizacja.HTM");
        var single = Touch("osobny", "egzamin.html");

        var found = StudyFiles.Find([
            Pin(PinType.Folder, Path.GetDirectoryName(quizzes)!, "Notatki BD2"),
            Pin(PinType.File, single, "Egzamin próbny"),
        ]);

        Assert.Equal(["normalizacja", "quiz-sql", "egzamin"], found.Select(f => f.Title));
        Assert.Equal("fiszki", found[0].Detail);
        Assert.Equal("fiszki/normalizacja.HTM", found[0].RelativePath);
        Assert.Equal("Notatki BD2", found[0].Group);
        Assert.Equal("", found[1].Detail);
        Assert.Equal(Path.GetDirectoryName(single), found[2].Root);
    }

    [Fact]
    public void Find_SkipsToolingFoldersHiddenFilesAndDuplicates()
    {
        var quiz = Touch("Sieci", "quiz.html");
        Touch("Sieci", "node_modules", "pkg", "readme.html");
        Touch("Sieci", ".git", "index.html");
        Touch("Sieci", "~$quiz.html");

        var folder = Path.GetDirectoryName(quiz)!;
        var found = StudyFiles.Find([
            Pin(PinType.Folder, folder, "Sieci"),
            Pin(PinType.File, quiz, "Quiz"),
            Pin(PinType.Folder, Path.Combine(_root, "brak"), "Przeniesiony folder"),
        ]);

        Assert.Equal([quiz], found.Select(f => f.FullPath));
    }

    [Fact]
    public void Find_StopsAtMaxDepth()
    {
        var deep = Enumerable.Repeat("x", StudyFiles.MaxDepth + 1).Append("za-gleboko.html").ToArray();
        Touch(["MSI", .. deep]);
        Touch(["MSI", .. deep.Skip(1)]);

        var found = StudyFiles.Find([Pin(PinType.Folder, Path.Combine(_root, "MSI"), "MSI")]);

        Assert.Single(found);
    }

    [Fact]
    public void VirtualHost_IsStableValidAndIgnoresCaseAndTrailingSlash()
    {
        var host = StudyFiles.VirtualHost(@"C:\Studia\BD2");

        Assert.Equal(host, StudyFiles.VirtualHost(@"c:\studia\bd2\"));
        Assert.NotEqual(host, StudyFiles.VirtualHost(@"C:\Studia\PLC"));
        Assert.Matches("^f[0-9a-f]{12}\\.nauka\\.learnonme$", host);
    }

    [Fact]
    public void VirtualUrl_EscapesEachSegment()
    {
        var file = new StudyFile("Quiz", "", _root, "Wykład 3/quiz łączenia.html", "BD2");

        var url = StudyFiles.VirtualUrl(file);

        Assert.Equal(StudyFiles.VirtualHost(_root), url.Host);
        Assert.Equal("/Wyk%C5%82ad%203/quiz%20%C5%82%C4%85czenia.html", url.AbsolutePath);
    }

    private string Touch(params string[] parts)
    {
        var path = Path.Combine([_root, .. parts]);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "<html></html>");
        return path;
    }

    private static Pin Pin(PinType type, string target, string title) =>
        new() { Type = type, Target = target, Title = title };
}
