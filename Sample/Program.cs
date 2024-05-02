using Jarvis.Application.Events;
using Jarvis.Infrastructure.DistributedEvent.RabbitMQ;
using Jarvis.Application;
using Jarvis.Persistence;
using Jarvis.WebApi;
using Jarvis.WebApi.Monitoring;
using Jarvis.WebApi.Monitoring.Uptrace;
using Sample.DataStorage;
using Sample.EventBus;
using Sample.DataStorage.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
OTLPType.TraceExporters.Add(OTLPOption.ExporterType.OTLP, typeof(OTLPTraceExporter).AssemblyQualifiedName);
OTLPType.MetricExporters.Add(OTLPOption.ExporterType.OTLP, typeof(OTLPMetricExporter).AssemblyQualifiedName);
OTLPType.LoggingExporters.Add(OTLPOption.ExporterType.OTLP, typeof(OTLPLogExporter).AssemblyQualifiedName);
var optionMonitor = builder.Services.BuildOptionMonitor(builder.Configuration);
builder.Services
    .AddCoreMonitor(optionMonitor)
    .AddCoreTrace(optionMonitor)
    .AddCoreMetric(optionMonitor)
    .AddCoreLogging(optionMonitor);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCoreApplication();
builder.Services.AddCoreWebApi(builder.Configuration);
builder.Services.AddCorePersistence(builder.Configuration);

builder.Services.AddEFMultiTenancy();
builder.Services.AddEFTenantDbContext();
builder.Services.AddEFSampleDbContext();

builder.Services.AddRabbitMQ(builder.Configuration);

// builder.Services.AddHostedService<SampleHostedService>();
builder.Services.AddTransient<IEvent<SampleEto>, SampleLocalEventHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
