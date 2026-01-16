using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
// Note: `UseWindowsService()` requires the Windows Services hosting package.
// For development we run the service in-console. When installing as a Windows
// service, add the Microsoft.Extensions.Hosting.WindowsServices package and
// uncomment the line below.
// builder.Host.UseWindowsService();
builder.WebHost.UseUrls("http://localhost:5005");

var connFile = Path.Combine(AppContext.BaseDirectory, "selfassistant.db");
builder.Configuration["Data:Sqlite:File"] = connFile;

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SelfAssistant.Service");

// Ensure DB exists
using (var c = new SqliteConnection($"Data Source={connFile}"))
{
    c.Open();
    c.Execute(@"CREATE TABLE IF NOT EXISTS Messages (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Role TEXT NOT NULL,
        Text TEXT NOT NULL,
        CreatedAt TEXT NOT NULL
    );");
}

// Simple API Key middleware. Set environment variable SELFASSISTANT_API_KEY to require a key.
var apiKey = Environment.GetEnvironmentVariable("SELFASSISTANT_API_KEY");
if (!string.IsNullOrEmpty(apiKey))
{
    app.Use(async (ctx, next) =>
    {
        if (!ctx.Request.Headers.TryGetValue("X-Api-Key", out var provided) || provided != apiKey)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsync("Unauthorized");
            return;
        }
        await next();
    });
}

// Restrict CORS to localhost for safety
app.UseCors(policy => policy.WithOrigins("http://localhost").AllowAnyMethod().AllowAnyHeader());

app.MapPost("/chat", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var text = await reader.ReadToEndAsync();
    if (string.IsNullOrWhiteSpace(text)) return Results.BadRequest();

    using var c = new SqliteConnection($"Data Source={connFile}");
    c.Open();
    var now = DateTime.UtcNow.ToString("o");
    c.Execute("INSERT INTO Messages (Role, Text, CreatedAt) VALUES (@Role, @Text, @CreatedAt)", new { Role = "user", Text = text, CreatedAt = now });

    // For scaffold/demo: create a simple assistant response (echo)
    var assistantText = "Assistant reply: " + text;
    c.Execute("INSERT INTO Messages (Role, Text, CreatedAt) VALUES (@Role, @Text, @CreatedAt)", new { Role = "assistant", Text = assistantText, CreatedAt = now });

    logger.LogInformation("Received chat message and queued assistant response.");
    return Results.Ok();
});

app.MapGet("/chat", () =>
{
    using var c = new SqliteConnection($"Data Source={connFile}");
    c.Open();
    var msg = c.QueryFirstOrDefault<string>("SELECT Text FROM Messages WHERE Role = 'assistant' ORDER BY Id LIMIT 1");
    if (msg is null) return Results.NoContent();
    // delete the message after retrieving
    c.Execute("DELETE FROM Messages WHERE Id IN (SELECT Id FROM Messages WHERE Role = 'assistant' ORDER BY Id LIMIT 1)");
    return Results.Ok(msg);
});

app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
