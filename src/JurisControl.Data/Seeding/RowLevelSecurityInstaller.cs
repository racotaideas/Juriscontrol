using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Data.Seeding;

/// <summary>
/// Aplica los scripts SQL de Row Level Security después de las migraciones EF.
/// Los scripts viven como <em>embedded resources</em> en el proyecto Data bajo
/// <c>Migrations/CustomSql/</c>. Cada uno debe ser idempotente (CREATE OR ALTER,
/// IF NOT EXISTS) porque se ejecuta en cada arranque.
/// </summary>
public static class RowLevelSecurityInstaller
{
    public static async Task ApplyAsync(JurisControlDbContext db)
    {
        var assembly = typeof(RowLevelSecurityInstaller).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.Contains(".Migrations.CustomSql.") && n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n)
            .ToArray();

        foreach (var name in resourceNames)
        {
            await using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync();

            foreach (var batch in SplitGoBatches(sql))
            {
                var trimmed = batch.Trim();
                if (trimmed.Length > 0)
                    await db.Database.ExecuteSqlRawAsync(trimmed);
            }
        }
    }

    private static IEnumerable<string> SplitGoBatches(string sql)
    {
        // SQL Server no acepta GO como parte del batch en ADO.NET; hay que partir.
        var lines = sql.Split('\n');
        var current = new List<string>();
        foreach (var raw in lines)
        {
            var line = raw.TrimEnd('\r');
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                yield return string.Join('\n', current);
                current.Clear();
            }
            else
            {
                current.Add(line);
            }
        }
        if (current.Count > 0) yield return string.Join('\n', current);
    }
}
