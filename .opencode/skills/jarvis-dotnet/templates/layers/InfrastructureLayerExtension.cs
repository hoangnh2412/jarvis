using {Product}.Domain.DependencyInjection;
using {Product}.Domain.Repositories;
using {Product}.Infrastructure.Persistence;
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
    builder.AddEntityFramework();

    builder.Services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();
    builder.Services.AddKeyedConfigConnectionStringResolver();

    builder.Services.AddCoreDbContext<AppDbContext, ConfigConnectionStringResolver>(
      (options, connectionString) => options.UseNpgsql(connectionString));

    return builder;
  }
}
