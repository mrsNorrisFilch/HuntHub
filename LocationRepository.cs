using Npgsql;
using NpgsqlTypes;

namespace LocationInfoService;

/// <summary>Text stored for a location.</summary>
public sealed record LocationResponse(string Location, string Description);

public static class LocationRepositoryLimits
{
    public const int MaxLocationLength = 100;
}

public interface ILocationRepository
{
    Task<LocationResponse?> GetDescriptionAsync(string location, CancellationToken ct);
}

public sealed class PostgresLocationRepository : ILocationRepository, IDisposable
{
    private const string Query =
        "SELECT name, description FROM locations WHERE name = @name LIMIT 1";

    private readonly NpgsqlDataSource _dataSource;

    public PostgresLocationRepository(IConfiguration configuration)
    {
        var db = configuration.GetSection("Database");
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Required(db, "Host"),
            Port = db.GetValue("Port", 5432),
            Database = Required(db, "Name"),
            Username = Required(db, "User"),
            Password = db["Password"],
        };
        _dataSource = NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<LocationResponse?> GetDescriptionAsync(string location, CancellationToken ct)
    {
        await using var command = _dataSource.CreateCommand(Query);
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar, LocationRepositoryLimits.MaxLocationLength) { Value = location });

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new LocationResponse(reader.GetString(0), reader.GetString(1));
    }

    public void Dispose() => _dataSource.Dispose();

    private static string Required(IConfigurationSection section, string key) =>
        section[key] ?? throw new InvalidOperationException($"Configuration value 'Database:{key}' is not set.");
}
