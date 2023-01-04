using Infrastructure.Caching.Redis;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database;
using Jarvis.Core.Database.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JarvisPresentation
{
    public static class ServiceCollectionExtension
    {
        public static void AddRedis(this IServiceCollection services, IConfigurationSection configSection)
        {
            services.AddRedisCache(options =>
            {
                var redisOption = new RedisOption();
                configSection.Bind(redisOption);

                options.InstanceName = redisOption.InstanceName;
                options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
                {
                    Password = redisOption.Password,
                    ConnectRetry = redisOption.ConnectRetry,
                    AbortOnConnectFail = redisOption.AbortOnConnectFail,
                    ConnectTimeout = redisOption.ConnectTimeout,
                    SyncTimeout = redisOption.SyncTimeout,
                    DefaultDatabase = redisOption.DefaultDatabase,
                };

                foreach (var item in redisOption.EndPoints)
                {
                    options.ConfigurationOptions.EndPoints.Add(item);
                }
            });
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
