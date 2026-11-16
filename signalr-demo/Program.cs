using SignalRDemo.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRDemoCors", policy =>
    {
        // Demo only. Restrict this to your frontend origin in production.
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("SignalRDemoCors");

app.MapGet("/", () => Results.Ok(new
{
    application = "Sliplane SignalR Demo",
    status = "running",
    hub = "/hubs/demo",
    health = "/health"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "signalr-demo"
}));

app.MapHub<DemoHub>("/hubs/demo");

app.Run();
