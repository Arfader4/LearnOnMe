using LearnOnMe.Core.Models;
using LearnOnMe.Core.Pins;

namespace LearnOnMe.Core.Tests;

public sealed class PinTargetsTests
{
    [Theory]
    [InlineData("drive.google.com/drive/folders/abc", "https://drive.google.com/drive/folders/abc")]
    [InlineData("  https://teams.microsoft.com/l/team/x  ", "https://teams.microsoft.com/l/team/x")]
    [InlineData("http://192.168.1.1", "http://192.168.1.1")]
    [InlineData("mailto:prowadzacy@uczelnia.pl", "mailto:prowadzacy@uczelnia.pl")]
    public void NormalizeLink_AddsHttpsOnlyWhenSchemeIsMissing(string input, string expected) =>
        Assert.Equal(expected, PinTargets.NormalizeLink(input));

    [Fact]
    public void NormalizePath_StripsCopyAsPathQuotes() =>
        Assert.Equal(@"C:\Studia\BD2", PinTargets.NormalizePath("\"C:\\Studia\\BD2\" "));

    [Theory]
    [InlineData(@"C:\Program Files\Cisco Packet Tracer 8.2.2\bin\PacketTracer.exe", PinType.App)]
    [InlineData(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\SSMS 21.lnk", PinType.App)]
    [InlineData(@"C:\Studia\CCNA\lab3.pkt", PinType.File)]
    [InlineData(@"C:\Studia\BD2\wyklad.PDF", PinType.File)]
    public void GuessType_RecognisesProgramsAndShortcuts(string path, PinType expected) =>
        Assert.Equal(expected, PinTargets.GuessType(path, isDirectory: false));

    [Fact]
    public void GuessType_DirectoryIsFolder() =>
        Assert.Equal(PinType.Folder, PinTargets.GuessType(@"C:\Studia\PLC", isDirectory: true));

    [Theory]
    [InlineData(PinType.Folder, @"C:\Studia\Sieci CCNA\", "Sieci CCNA")]
    [InlineData(PinType.File, @"C:\Studia\BD2\wyklad 3.pdf", "wyklad 3")]
    [InlineData(PinType.App, @"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\devenv.exe", "devenv")]
    [InlineData(PinType.Folder, @"D:\", @"D:\")]
    [InlineData(PinType.Link, "https://www.netacad.com/courses", "netacad.com")]
    [InlineData(PinType.Link, "uczelnia-my.sharepoint.com/personal/x", "uczelnia-my.sharepoint.com")]
    public void SuggestTitle_UsesNameOrHost(PinType type, string target, string expected) =>
        Assert.Equal(expected, PinTargets.SuggestTitle(type, target));

    [Fact]
    public void Exists_ChecksFoldersAndFilesButNotLinks()
    {
        var file = Path.GetTempFileName();
        try
        {
            Assert.True(PinTargets.Exists(PinType.File, file));
            Assert.True(PinTargets.Exists(PinType.Folder, Path.GetDirectoryName(file)!));
            Assert.False(PinTargets.Exists(PinType.App, file + ".missing.exe"));
            Assert.True(PinTargets.Exists(PinType.Link, "https://example.org"));
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void CreateStartInfo_ShellExecutesNormalizedLink()
    {
        var info = PinTargets.CreateStartInfo(
            new Pin { Type = PinType.Link, Title = "Drive", Target = "drive.google.com/x" });

        Assert.True(info.UseShellExecute);
        Assert.Equal("https://drive.google.com/x", info.FileName);
        Assert.Equal("", info.WorkingDirectory);
    }

    [Fact]
    public void CreateStartInfo_AppStartsInItsOwnFolder()
    {
        var exe = Path.Combine(Path.GetTempPath(), "PAC", "ME.exe");

        var info = PinTargets.CreateStartInfo(new Pin { Type = PinType.App, Title = "PAC", Target = exe });

        Assert.Equal(exe, info.FileName);
        Assert.Equal(Path.GetDirectoryName(exe), info.WorkingDirectory);
    }
}
