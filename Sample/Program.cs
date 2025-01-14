using Sample;
using Jarvis.OpenTelemetry;
using Sample.Persistence;
using Jarvis.EntityFramework;
using Jarvis.Mvc;
using Jarvis.Mvc.ExceptionHandling;
using Jarvis.Swashbuckle;
using Jarvis.Mvc.ApplicationBuilders;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTelemetry(builder.Configuration, (services) => { })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace()
    .ConfigureMetric();

builder.AddCoreSwagger();
builder.AddCoreJson();
builder.AddCoreCors();

builder.AddCoreWebApi();
builder.AddEntityFramework();
builder.AddSampleDbContext();

// builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.UseCoreSwagger();
app.UseHttpsRedirection();
app.MapControllers();
app.UseCoreCors();

app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();
app.EnsureMigrateDb<ISampleUnitOfWork>();
app.Run();