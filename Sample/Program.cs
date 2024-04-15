using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence;
using Jarvis.Persistence.DataContexts;
using Sample.DataStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCorePersistence(builder.Configuration);
builder.Services.AddMultiTenancy();
builder.Services.AddTenantDbContext();
builder.Services.AddSampleDbContext();

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
