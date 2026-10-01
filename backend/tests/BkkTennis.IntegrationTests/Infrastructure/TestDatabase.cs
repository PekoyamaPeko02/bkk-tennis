using Npgsql;

namespace BkkTennis.IntegrationTests.Infrastructure;

/// <summary>
/// A throw-away PostGIS database per test run, built from the same <c>db/init/*.sql</c> files as production
/// (schema + seed). Connection is configurable so CI (service DB on 5432) and local dev (docker compose on 5433) both work:
/// TEST_DB_HOST, TEST_DB_PORT, TEST_DB_USER, TEST_DB_PASSWORD. The user must be allowed to CREATE DATABASE and CREATE EXTENSION.
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly NpgsqlConnectionStringBuilder _admin;

    public string Name { get; } = $"bkktennis_test_{Guid.NewGuid():N}";
    public string ConnectionString { get; }

    private TestDatabase()
    {
        _admin = new NpgsqlConnectionStringBuilder
        {
            Host = Env("TEST_DB_HOST", "localhost"),
            Port = int.Parse(Env("TEST_DB_PORT", "5433")),
            Username = Env("TEST_DB_USER", "bkktennis"),
            Password = Env("TEST_DB_PASSWORD", "bkktennis"),
            Database = "postgres",
            Pooling = false,
        };
        ConnectionString = new NpgsqlConnectionStringBuilder(_admin.ConnectionString) { Database = Name, Pooling = true }.ConnectionString;
    }

    public static async Task<TestDatabase> CreateAsync()
    {
        var db = new TestDatabase();
        await Exec(db._admin.ConnectionString, $"CREATE DATABASE \"{db.Name}\"");
        try
        {
            var direct = new NpgsqlConnectionStringBuilder(db.ConnectionString) { Pooling = false }.ConnectionString;
            foreach (var file in Directory.GetFiles(FindInitDir(), "*.sql").OrderBy(f => f, StringComparer.Ordinal))
                await Exec(direct, await File.ReadAllTextAsync(file));
        }
        catch
        {
            await db.DisposeAsync();
            throw;
        }
        return db;
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await Exec(_admin.ConnectionString, $"DROP DATABASE IF EXISTS \"{Name}\" WITH (FORCE)");
    }

    private static async Task Exec(string connectionString, string sql)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private static string Env(string name, string fallback) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } v ? v : fallback;

    private static string FindInitDir()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "db", "init");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("db/init not found above " + AppContext.BaseDirectory);
    }
}
