// Scaffold Infrastructure — mở rộng: jarvis-dotnet/templates/SKILLS.md
//   caching-dotnet (AddJarvisCaching trước EF) | entityframework-dotnet (patterns/)
//   blobstoring-dotnet | notification-dotnet (thường Host hoặc Infrastructure)

using {Product}.Domain.DependencyInjection;
using {Product}.Domain.Repositories;
using {Product}.Infrastructure.Persistence;
using Jarvis.Caching.Extensions;
using Jarvis.Domain.DataStorages;
using Jarvis.EntityFramework;
using Jarvis.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace {Product}.Infrastructure.DependencyInjection;

public static class InfrastructureLayerExtension
{
  public static IHostApplicationBuilder AddInfrastructureLayer(this IHostApplicationBuilder builder)
  {
    builder.AddDomainLayer();
    builder.AddJarvisCaching();
    builder.AddEntityFramework();

    builder.Services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();

    builder.Services.AddCoreDbContext<AppDbContext, ConfigConnectionStringResolver>(options =>
      options.UseNpgsql("Host=localhost;Database=placeholder"));

    return builder;
  }
}
