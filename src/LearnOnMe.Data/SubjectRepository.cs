using LearnOnMe.Core.Abstractions;
using LearnOnMe.Core.Models;
using Microsoft.Data.Sqlite;

namespace LearnOnMe.Data;

public sealed class SubjectRepository(LearnOnMeDatabase db) : ISubjectRepository
{
    private const string SelectColumns = "SELECT Id, Code, Name, AccentColor, SortOrder FROM Subjects";

    public IReadOnlyList<Subject> GetAll()
    {
        using var connection = db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = SelectColumns + " ORDER BY SortOrder, Id;";
        using var reader = cmd.ExecuteReader();
        var result = new List<Subject>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }
        return result;
    }

    public Subject? GetById(long id)
    {
        using var connection = db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = SelectColumns + " WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public long Add(Subject subject)
    {
        using var connection = db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Subjects (Code, Name, AccentColor, SortOrder)
            VALUES ($code, $name, $color, $order);
            SELECT last_insert_rowid();
            """;
        Bind(cmd, subject);
        subject.Id = (long)cmd.ExecuteScalar()!;
        return subject.Id;
    }

    public void Update(Subject subject)
    {
        using var connection = db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Subjects
            SET Code = $code, Name = $name, AccentColor = $color, SortOrder = $order
            WHERE Id = $id;
            """;
        Bind(cmd, subject);
        cmd.Parameters.AddWithValue("$id", subject.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(long id)
    {
        using var connection = db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Subjects WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private static void Bind(SqliteCommand cmd, Subject subject)
    {
        cmd.Parameters.AddWithValue("$code", subject.Code);
        cmd.Parameters.AddWithValue("$name", subject.Name);
        cmd.Parameters.AddWithValue("$color", subject.AccentColor);
        cmd.Parameters.AddWithValue("$order", subject.SortOrder);
    }

    private static Subject Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt64(0),
        Code = r.GetString(1),
        Name = r.GetString(2),
        AccentColor = r.GetString(3),
        SortOrder = r.GetInt32(4),
    };
}
