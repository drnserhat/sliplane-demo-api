using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = builder.Configuration["DATABASE_URL"];
var redisUrl = builder.Configuration["REDIS_URL"];

if (string.IsNullOrWhiteSpace(databaseUrl))
    throw new InvalidOperationException("DATABASE_URL environment variable is required.");

if (string.IsNullOrWhiteSpace(redisUrl))
    throw new InvalidOperationException("REDIS_URL environment variable is required.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisUrl));

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    application = "Sliplane Demo API",
    status = "running",
    endpoints = new[] { "/health", "/health/db", "/health/cache", "/demo/cache" }
}));

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/health/db", async (CancellationToken cancellationToken) =>
{
    try
    {
        await using var connection = new NpgsqlConnection(databaseUrl);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand("SELECT version();", connection);
        var version = (string?)await command.ExecuteScalarAsync(cancellationToken);

        return Results.Ok(new { status = "connected", database = "PostgreSQL", version });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "PostgreSQL connection failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/health/cache", async (IConnectionMultiplexer redis) =>
{
    try
    {
        var database = redis.GetDatabase();
        var latency = await database.PingAsync();

        return Results.Ok(new
        {
            status = "connected",
            cache = "Garnet (Redis-compatible)",
            pingMilliseconds = Math.Round(latency.TotalMilliseconds, 2)
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Garnet connection failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/demo/cache", async (IConnectionMultiplexer redis) =>
{
    var database = redis.GetDatabase();
    const string key = "sliplane-demo:message";

    await database.StringSetAsync(key, "Hello from ASP.NET Core + Garnet!", TimeSpan.FromMinutes(10));
    var value = await database.StringGetAsync(key);

    return Results.Ok(new { key, value = value.ToString(), ttlMinutes = 10 });
});

app.MapPost("/demo/db/seed", async () =>
{
    await using var connection = new NpgsqlConnection(databaseUrl);
    await connection.OpenAsync();

    const string sql = """
        CREATE TABLE IF NOT EXISTS persistence_test (
            id INTEGER PRIMARY KEY,
            message TEXT NOT NULL,
            created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );

        INSERT INTO persistence_test (id, message)
        VALUES (1, 'Sliplane Volume test successful')
        ON CONFLICT (id) DO NOTHING;
        """;

    await using var command = new NpgsqlCommand(sql, connection);
    await command.ExecuteNonQueryAsync();

    return Results.Ok(new { status = "inserted", id = 1 });
});

app.MapGet("/demo/db/read", async () =>
{
    await using var connection = new NpgsqlConnection(databaseUrl);
    await connection.OpenAsync();

    const string sql = """
        SELECT id, message, created_at
        FROM persistence_test
        WHERE id = 1;
        """;

    await using var command = new NpgsqlCommand(sql, connection);
    await using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return Results.NotFound(new { status = "not_found" });

    return Results.Ok(new
    {
        id = reader.GetInt32(0),
        message = reader.GetString(1),
        createdAt = reader.GetDateTime(2)
    });
});

app.Run();
