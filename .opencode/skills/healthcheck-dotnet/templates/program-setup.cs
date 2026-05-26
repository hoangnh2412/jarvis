// Program.cs — minimal Jarvis healthcheck wiring
// Replace {App} with your application namespace.

using Jarvis.HealthChecks;
using {App}.Health;

var builder = WebApplication.CreateBuilder(args);

// Core: liveness + startup (Jarvis)
builder.AddHealthChecks();

// Host-owned readiness (optional — create extension per init workflow)
builder.Add{App}ReadinessHealthChecks();

var app = builder.Build();

app.MapControllers(); // or other endpoints

// Must be called — maps /health/live, /health/ready, /health/startup, /health
app.UseHealthChecks();

// If HealthChecks:MarkStartupCompleteOnApplicationStarted = false:
// await app.Services.GetRequiredService<IStartupCompletionNotifier>().MarkStartupCompleteAsync();

app.Run();
