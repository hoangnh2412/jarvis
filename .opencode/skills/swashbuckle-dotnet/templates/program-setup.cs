using Asp.Versioning;
using Jarvis.Mvc;
using Jarvis.Swashbuckle;

var builder = WebApplication.CreateBuilder(args);

builder.AddCoreWebApi();
builder.Services.AddApiVersioning(options =>
{
  options.DefaultApiVersion = new ApiVersion(1, 0);
  options.AssumeDefaultVersionWhenUnspecified = true;
  options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
  options.GroupNameFormat = "'v'VVV";
  options.SubstituteApiVersionInUrl = true;
});

builder.AddCoreSwagger();

var app = builder.Build();
app.UseCoreSwagger();
app.MapControllers();
app.Run();
