using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.DataStorages;
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
        using (TenantScopedContextValidation.BeginSuppressScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<T>();
            var dbContext = uow.GetDbContextAsync().GetAwaiter().GetResult() as DbContext;
            dbContext?.Database.MigrateAsync().GetAwaiter().GetResult();
        }

        return builder;
    }
}