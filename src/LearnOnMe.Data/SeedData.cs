using LearnOnMe.Core.Models;
using Microsoft.Data.Sqlite;

namespace LearnOnMe.Data;

/// <summary>Starting subjects inserted once, when the database is created.</summary>
public static class SeedData
{
    public static IReadOnlyList<Subject> Subjects { get; } =
    [
        new() { Code = "BD2",  Name = "BD2",                  AccentColor = "#4F8CFF" },
        new() { Code = "PIV",  Name = "Programowanie IV",     AccentColor = "#A66BFF" },
        new() { Code = "CCNA", Name = "Sieci (CCNA)",         AccentColor = "#1FB5A6" },
        new() { Code = "MSI",  Name = "MSI",                  AccentColor = "#FF8A3D" },
        new() { Code = "MO",   Name = "Metody optymalizacji", AccentColor = "#F2C94C" },
        new() { Code = "PLC",  Name = "PLC",                  AccentColor = "#6FCF6F" },
        new() { Code = "SMiS", Name = "SMiS",                 AccentColor = "#EB5A8C" },
        new() { Code = "CYB",  Name = "Cyberbezpieczeństwo",  AccentColor = "#EF5350" },
        new() { Code = "USN",  Name = "Usługi sieciowe",      AccentColor = "#38B6E8" },
    ];

    internal static void Apply(SqliteConnection connection)
    {
        using var tx = connection.BeginTransaction();
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText =
            "INSERT INTO Subjects (Code, Name, AccentColor, SortOrder) VALUES ($code, $name, $color, $order);";
        var code = cmd.Parameters.Add("$code", SqliteType.Text);
        var name = cmd.Parameters.Add("$name", SqliteType.Text);
        var color = cmd.Parameters.Add("$color", SqliteType.Text);
        var order = cmd.Parameters.Add("$order", SqliteType.Integer);

        for (var i = 0; i < Subjects.Count; i++)
        {
            code.Value = Subjects[i].Code;
            name.Value = Subjects[i].Name;
            color.Value = Subjects[i].AccentColor;
            order.Value = i;
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }
}
