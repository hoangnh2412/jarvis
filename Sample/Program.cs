using Sample;
using Jarvis.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCoreMonitor(builder.Configuration, (services) => { })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace()
    .ConfigureMetric();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async () =>
{
    var threadId = System.Environment.CurrentManagedThreadId;
    var xxx = Thread.CurrentThread.ManagedThreadId;

    Console.WriteLine($"Thread: {Environment.CurrentManagedThreadId} | Task: {Task.CurrentId}");

    await Task.Run(() => Console.WriteLine($"Thread: {Environment.CurrentManagedThreadId} | Task: {Task.CurrentId}"));

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    using (var scope = logger.BeginScope("Begin"))
    {
        logger.LogInformation("haha");
    }

    logger.LogInformation("hihi");

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
