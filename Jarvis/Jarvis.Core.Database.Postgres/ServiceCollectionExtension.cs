using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Core.Database.Postgres
{
    public static class ServiceCollectionExtension
    {
        public static void AddCoreDbContext(this IServiceCollection services, string connectionString)
        {
            InstanceNames.DbContexts.Add(typeof(CoreDbContext).AssemblyQualifiedName, typeof(CoreDbContext));
            services.AddScoped<CoreDbContext>();

            services.AddDbContextPool<CoreDbContext>(options =>
            {
                options.UseNpgsql(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__core_migration_history");
                });
            });
            services.AddScoped<ICoreUnitOfWork, CoreUnitOfWork>();
        }
    }
}