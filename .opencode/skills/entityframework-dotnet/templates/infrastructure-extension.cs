// {Product}.Infrastructure — EF + Caching (đổi pattern trong AddAppDbContext)

using Jarvis.Caching.Extensions;
using Jarvis.EntityFramework;
using Jarvis.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;

public static class InfrastructureLayerExtension
{
  public static IHostApplicationBuilder AddInfrastructureLayer(this IHostApplicationBuilder builder)
  {
    builder.AddJarvisCaching();
    builder.AddEntityFramework();

    builder.Services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();

    // Single DB — xem patterns/single-db/SKILL.md
    builder.Services.AddCoreDbContext<AppDbContext, ConfigConnectionStringResolver>(options =>
      options.UseNpgsql("Host=localhost;Database=placeholder"));

    return builder;
  }
}
