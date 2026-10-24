using LearnOnMe.Core.Abstractions;
using LearnOnMe.Core.Models;
using Microsoft.Data.Sqlite;

namespace LearnOnMe.Data;

public sealed class PinRepository(LearnOnMeDatabase db) : IPinRepository
{
    public IReadOnlyList<Pin> GetForSubject(long subjectId)
    {
        using var connection = db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, SubjectId, Type, Title, Target, SortOrder
            FROM Pins WHERE SubjectId = $subjectId
            ORDER BY SortOrder, Id;
            """;
        cmd.Parameters.AddWithValue("$subjectId", subjectId);
        using var reader = cmd.ExecuteReader();
        var result = new List<Pin>();
        while (reader.Read())
        {
            result.Add(new Pin
            {
                Id = reader.GetInt64(0),
                SubjectId = reader.GetInt64(1),
                Type = (PinType)reader.GetInt32(2),
                Title = reader.GetString(3),
                Target = reader.GetString(4),
                SortOrder = reader.GetInt32(5),
            });
        }
        return result;
    }

    public long Add(Pin pin)
    {
        using var connection = db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Pins (SubjectId, Type, Title, Target, SortOrder)
            VALUES ($subjectId, $type, $title, $target, $order);
            SELECT last_insert_rowid();
            """;
        Bind(cmd, pin);
        pin.Id = (long)cmd.ExecuteScalar()!;
        return pin.Id;
    }

    public void Update(Pin pin)
    {
        using var connection = db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Pins
            SET SubjectId = $subjectId, Type = $type, Title = $title, Target = $target, SortOrder = $order
            WHERE Id = $id;
            """;
        Bind(cmd, pin);
        cmd.Parameters.AddWithValue("$id", pin.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(long id)
    {
        using var connection = db.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Pins WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private static void Bind(SqliteCommand cmd, Pin pin)
    {
        cmd.Parameters.AddWithValue("$subjectId", pin.SubjectId);
        cmd.Parameters.AddWithValue("$type", (int)pin.Type);
        cmd.Parameters.AddWithValue("$title", pin.Title);
        cmd.Parameters.AddWithValue("$target", pin.Target);
        cmd.Parameters.AddWithValue("$order", pin.SortOrder);
    }
}
