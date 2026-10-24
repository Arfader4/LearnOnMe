using Microsoft.Data.Sqlite;

namespace LearnOnMe.Data;

/// <summary>
/// Owns the SQLite connection string and brings the schema up to date.
/// Schema version is kept in PRAGMA user_version; add new steps to <see cref="Migrations"/>.
/// </summary>
public sealed class LearnOnMeDatabase
{
    private static readonly string[] Migrations =
    [
        // v1: subjects, pins, settings
        """
        CREATE TABLE Subjects (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            Code        TEXT    NOT NULL,
            Name        TEXT    NOT NULL,
            AccentColor TEXT    NOT NULL,
            SortOrder   INTEGER NOT NULL DEFAULT 0
        );
        CREATE TABLE Pins (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            SubjectId INTEGER NOT NULL REFERENCES Subjects(Id) ON DELETE CASCADE,
            Type      INTEGER NOT NULL,
            Title     TEXT    NOT NULL,
            Target    TEXT    NOT NULL,
            SortOrder INTEGER NOT NULL DEFAULT 0
        );
        CREATE INDEX IX_Pins_SubjectId ON Pins(SubjectId);
        CREATE TABLE Settings (
            Key   TEXT PRIMARY KEY,
            Value TEXT NOT NULL
        );
        """,
    ];

    private readonly string _connectionString;

    public LearnOnMeDatabase(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true,
        }.ToString();
    }

    /// <summary>Default location: %LOCALAPPDATA%\LearnOnMe\learnonme.db.</summary>
    public static string DefaultPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LearnOnMe");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "learnonme.db");
    }

    public static int SchemaVersion => Migrations.Length;

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <summary>Applies pending migrations and seeds starting data on a fresh database.</summary>
    public void Initialize()
    {
        using var connection = Open();
        var version = Convert.ToInt32(Execute(connection, "PRAGMA user_version;"));
        var isFresh = version == 0;

        for (var i = version; i < Migrations.Length; i++)
        {
            using var tx = connection.BeginTransaction();
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = Migrations[i] + $"\nPRAGMA user_version = {i + 1};";
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
        }

        if (isFresh)
        {
            SeedData.Apply(connection);
        }
    }

    private static object? Execute(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar();
    }
}
