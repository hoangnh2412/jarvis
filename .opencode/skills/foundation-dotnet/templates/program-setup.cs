// Host — Jarvis foundation (minimal)
using Jarvis.Domain;
using Jarvis.Mvc;
using Jarvis.Mvc.ApplicationBuilders;
using Jarvis.Mvc.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

builder.AddCoreJson();
builder.AddCoreCors();
builder.AddCoreDomain();
builder.AddCoreWebApi();

var app = builder.Build();

app.UseCoreCors();
app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();
app.MapControllers();

app.Run();
