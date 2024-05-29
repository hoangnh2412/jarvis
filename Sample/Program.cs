using Jarvis.Application.Events;
using Jarvis.Application;
using Jarvis.Persistence;
using Jarvis.WebApi;
using Jarvis.WebApi.Monitoring;
using Sample.EventBus;
using Sample.DataStorage.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreMonitor(builder.Configuration)
    .ConfigureResource()
    .ConfigureTrace()
    .ConfigureMetric()
    .ConfigureLogging();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCoreApplication();
builder.Services.AddCoreWebApi(builder.Configuration);

builder.Services
    .AddCorePersistence(builder.Configuration)
    .AddEFRepositories()
    .AddEFMultiTenancy()
    .AddEFTenantDbContext()
    .AddEFSampleDbContext();

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
