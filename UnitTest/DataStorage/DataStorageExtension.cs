using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Persistence;

namespace UnitTest.DataStorage;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddSampleDbContext(this IServiceCollection services)
    {
        services.AddCoreDbContext<SampleDbContext>((connection, options) =>
        {
            options.UseNpgsql(connection);
        });
        services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();
        return services;
    }
}