using Npgsql;
using Platform.Multitenancy.EntityFramework;
using Platform.ORM.Dapper;
using Platform.ORM.EntityFramework;
using Sample.ImportExport;
using Sample.Multitenancy;
using Sample.Services;
using StackExchange.Redis;

namespace Sample.Extensions;

public static class SamplePersistenceExtensions
{
    public static WebApplicationBuilder AddSamplePersistence(this WebApplicationBuilder builder)
    {
        builder.AddEntityFramework();
        builder.AddMultitenancyEntityFramework();
        builder.AddOrmDapper(options =>
        {
            options.ConnectionStringName = "MasterDbContext";
            options.CreateConnection = connectionString => new NpgsqlConnection(connectionString);
        });
        builder.Services.AddScoped<SampleDapperReadSample>();
        builder.AddSampleDbContext();
        builder.AddSampleImportExport();
        builder.AddMultitenancyEfTestHostedService();

        // Redis (StackExchange) cho demo Sample — cùng key với Cache:DistributedGroups:Redis:Default.
        builder.Services.AddKeyedSingleton<IConnectionMultiplexer>("Default", (_, keyedService) =>
        {
            var configuration = _.GetRequiredService<IConfiguration>();
            var redisConfig = configuration["Cache:DistributedGroups:Redis:Default:Configuration"];
            return ConnectionMultiplexer.Connect(redisConfig!);
        });

        return builder;
    }
}
