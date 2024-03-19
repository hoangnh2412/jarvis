using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Shared.Options;

namespace Jarvis.Persistence;

public static class AppBuilderExtension
{
    public static IApplicationBuilder EnsureMigrateDb<T>(this IApplicationBuilder builder) where T : IUnitOfWork
    {
        var options = builder.ApplicationServices.GetService<IOptions<StorageContextOption>>().Value;
        if (!options.AutoMigrate)
            return builder;

        using (var scope = builder.ApplicationServices.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<T>();
            var dbContext = uow.GetDbContext() as DbContext;
            dbContext.Database.Migrate();
        }

        return builder;
    }
}