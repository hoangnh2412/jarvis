using Jarvis.Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jarvis.EntityFramework;

public static class AppBuilderExtension
{
    public static IApplicationBuilder EnsureMigrateDb<T>(this IApplicationBuilder builder) where T : IUnitOfWork
    {
        var options = builder.ApplicationServices.GetRequiredService<IOptions<StorageContextOption>>().Value;
        if (!options.AutoMigrate)
            return builder;

        using (var scope = builder.ApplicationServices.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<T>();
            var dbContext = (DbContext)uow.GetDbContext();
            dbContext.Database.Migrate();
        }

        return builder;
    }
}