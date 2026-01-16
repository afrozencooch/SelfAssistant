using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.WebHost.UseUrls("http://localhost:5005");
builder.Services.AddSingleton<ConcurrentQueue<string>>();

var app = builder.Build();

app.MapPost("/chat", async (HttpContext ctx, ConcurrentQueue<string> queue) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var text = await reader.ReadToEndAsync();
    if (!string.IsNullOrWhiteSpace(text)) queue.Enqueue(text);
    return Results.Ok();
});

app.MapGet("/chat", (ConcurrentQueue<string> queue) =>
{
    if (queue.TryDequeue(out var msg)) return Results.Ok(msg);
    return Results.NoContent();
});

app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
