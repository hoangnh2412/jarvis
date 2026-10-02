using Platform.Modules.ElsaWorkflow.Api.Extensions;
using Platform.Modules.ElsaWorkflow.EntityFramework.Extensions;
using Platform.Modules.ElsaWorkflow.Extensions;
using Platform.Caching.Extensions;
using Platform.Caching.Redis;
using Platform.Workflow.Extensions;
using Platform.ORM.EntityFramework;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Microsoft.EntityFrameworkCore;
using Platform.Workflow.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddPlatformCaching()
    .UseRedisDistributedCache();

var redisConfiguration = builder.Configuration["Cache:DistributedGroups:Redis:Default:Configuration"]
    ?? throw new InvalidOperationException("Cache:DistributedGroups:Redis:Default:Configuration is not configured.");

builder.Services.AddPlatformWorkflowServices(
    redisConfiguration,
    options => options.ConfigureFromSection(builder.Configuration));

// Standalone Workflow Server: engine, persistence, API và Studio cùng chạy trong host này.
builder.AddElsaWorkflowModule()
    .UseEntityFramework()
    .UseHttpApi()
    .UseStudio()
    .ApplyStandaloneServer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
// Host application owns the authentication scheme and authorization policies.
app.UseAuthentication();
app.UseAuthorization();
app.UseWorkflowApi();
app.UseWorkflowStudio();

var elsaWorkflowOptions = ElsaWorkflowOptions.FromConfiguration(app.Configuration);
var elsaConnectionString = app.Configuration.GetConnectionString(elsaWorkflowOptions.ConnectionStringName);
if (string.IsNullOrWhiteSpace(elsaConnectionString))
{
    throw new InvalidOperationException(
        $"Connection string '{elsaWorkflowOptions.ConnectionStringName}' is not configured or empty.");
}

app.EnsureMigrateTemplateDb<ManagementElsaDbContext>((configuration, options) =>
    options.UseNpgsql(elsaConnectionString));
app.EnsureMigrateTemplateDb<RuntimeElsaDbContext>((configuration, options) =>
    options.UseNpgsql(elsaConnectionString));

app.Run();
