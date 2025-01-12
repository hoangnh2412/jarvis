using Jarvis.Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.EntityFramework;

public static class AppBuilderExtension
{
    public static IApplicationBuilder EnsureMigrateDb<T>(this IApplicationBuilder builder) where T : IUnitOfWork
    {
        var configuration = builder.ApplicationServices.GetRequiredService<IConfiguration>();
        var enableAutoMigrate = configuration.GetValue<bool>("ConnectionStrings:AutoMigrate");
        if (!enableAutoMigrate)
            return builder;

        using (var scope = builder.ApplicationServices.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<T>();
            var dbContext = uow.GetDbContext() as DbContext;
            dbContext?.Database.Migrate();
        }

        return builder;
    }
}