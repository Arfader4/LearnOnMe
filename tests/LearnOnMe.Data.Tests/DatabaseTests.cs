using LearnOnMe.Core.Models;
using Microsoft.Data.Sqlite;

namespace LearnOnMe.Data.Tests;

public sealed class DatabaseTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"learnonme-{Guid.NewGuid():N}.db");
    private readonly LearnOnMeDatabase _db;

    public DatabaseTests()
    {
        _db = new LearnOnMeDatabase(_path);
        _db.Initialize();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }

    [Fact]
    public void Initialize_SeedsStartingSubjectsInOrder()
    {
        var names = new SubjectRepository(_db).GetAll().Select(s => s.Name).ToList();

        Assert.Equal(SeedData.Subjects.Select(s => s.Name), names);
        Assert.Contains("Cyberbezpieczeństwo", names);
    }

    [Fact]
    public void Initialize_IsIdempotent()
    {
        _db.Initialize();
        _db.Initialize();

        Assert.Equal(SeedData.Subjects.Count, new SubjectRepository(_db).GetAll().Count);
        using var connection = _db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA user_version;";
        Assert.Equal((long)LearnOnMeDatabase.SchemaVersion, cmd.ExecuteScalar());
    }

    [Fact]
    public void Subjects_CrudRoundTrip()
    {
        var repo = new SubjectRepository(_db);
        var id = repo.Add(new Subject { Code = "TST", Name = "Test", AccentColor = "#FFFFFF", SortOrder = 99 });

        var loaded = repo.GetById(id)!;
        loaded.Name = "Test 2";
        repo.Update(loaded);
        Assert.Equal("Test 2", repo.GetById(id)!.Name);

        repo.Delete(id);
        Assert.Null(repo.GetById(id));
    }

    [Fact]
    public void Pins_AreDeletedWithTheirSubject()
    {
        var subjects = new SubjectRepository(_db);
        var pins = new PinRepository(_db);
        var subjectId = subjects.GetAll()[0].Id;

        pins.Add(new Pin { SubjectId = subjectId, Type = PinType.Link, Title = "UPeL", Target = "https://example.org" });
        pins.Add(new Pin { SubjectId = subjectId, Type = PinType.Folder, Title = "Notatki", Target = @"C:\Studia\BD2" });
        Assert.Equal(2, pins.GetForSubject(subjectId).Count);
        Assert.Equal(PinType.Folder, pins.GetForSubject(subjectId)[1].Type);

        subjects.Delete(subjectId);
        Assert.Empty(pins.GetForSubject(subjectId));
    }

    [Fact]
    public void Pins_CrudRoundTrip()
    {
        var pins = new PinRepository(_db);
        var subjectId = new SubjectRepository(_db).GetAll()[0].Id;
        var pin = new Pin { SubjectId = subjectId, Type = PinType.App, Title = "SSMS", Target = @"C:\SSMS\Ssms.exe" };
        pins.Add(pin);

        pin.Title = "SQL Server Management Studio";
        pin.Type = PinType.Link;
        pin.Target = "https://learn.microsoft.com/sql/ssms";
        pins.Update(pin);
        var loaded = Assert.Single(pins.GetForSubject(subjectId));
        Assert.Equal("SQL Server Management Studio", loaded.Title);
        Assert.Equal(PinType.Link, loaded.Type);
        Assert.Equal("https://learn.microsoft.com/sql/ssms", loaded.Target);

        pins.Delete(pin.Id);
        Assert.Empty(pins.GetForSubject(subjectId));
    }

    [Fact]
    public void Settings_UpsertAndRead()
    {
        var settings = new SettingsRepository(_db);
        Assert.Null(settings.Get("theme"));

        settings.Set("theme", "dark");
        settings.Set("theme", "darker");

        Assert.Equal("darker", settings.Get("theme"));
    }
}
