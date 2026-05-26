// Host — authentication (JWT example)
using Jarvis.Authentication.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddCoreJwtBearer(builder.Configuration);

// builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
