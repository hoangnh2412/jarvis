using System;
using Infrastructure.Caching;
using Infrastructure.Caching.Redis;
using Infrastructure.Database;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database;
using Jarvis.Core.Database.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace JarvisPresentation
{
    public static class ServiceCollectionExtension
    {
        public static void AddRedis(this IServiceCollection services)
        {
            // var redisOption = new RedisOption();
            // Configuration.GetSection("Redis").Bind(redisOption);
            // services.AddRedis(redisOption);
        }

        public static void AddCoreDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IStorageContext>(provider => provider.GetService<CoreDbContext>());
            services.AddDbContextPool<CoreDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__CoreMigrationHistory");
                });
            });
            services.AddScoped<ICoreUnitOfWork, CoreUnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
        }

        public static void AddDapper(this IServiceCollection services)
        {
            // services.AddSingleton<IDapperConnector, DapperConnector>();
            // services.AddSingleton<IDapperRepository, OracleDapperRepository>();
        }
    }
}
