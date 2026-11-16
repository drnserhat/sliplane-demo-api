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

app.Run();
