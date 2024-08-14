using OpenTelemetry.Trace;
using Jarvis.OpenTelemetry.Interfaces;

namespace Jarvis.OpenTelemetry.Instrumentations;

public class RedisTraceInstrumentation : ITraceInstrumentation
{
    private readonly OTELOption _options;

    public RedisTraceInstrumentation(
        OTELOption options)
    {
        _options = options;
    }

    public TracerProviderBuilder AddInstrumentation(TracerProviderBuilder builder)
    {
        // var configuration = new ConfigurationBuilder()
        //     .SetBasePath(Directory.GetCurrentDirectory())
        //     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //     .AddJsonFile($"appsettings.{Environments.Development}.json", optional: true, reloadOnChange: true)
        //     .AddJsonFile($"appsettings.{Environments.Staging}.json", optional: true, reloadOnChange: true)
        //     .AddJsonFile($"appsettings.{Environments.Production}.json", optional: true, reloadOnChange: true)
        //     .AddEnvironmentVariables()
        //     .Build();

        // var redisOption = new RedisOption();
        // configuration.GetSection("Redis").Bind(redisOption);

        // var redisCacheOptions = new RedisCacheOptions();
        // redisCacheOptions.InstanceName = redisOption.InstanceName;
        // redisCacheOptions.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
        // {
        //     Password = redisOption.Password,
        //     ConnectRetry = redisOption.ConnectRetry,
        //     AbortOnConnectFail = redisOption.AbortOnConnectFail,
        //     ConnectTimeout = redisOption.ConnectTimeout,
        //     SyncTimeout = redisOption.SyncTimeout,
        //     DefaultDatabase = redisOption.DefaultDatabase,
        // };

        // if (redisOption.EndPoints != null)
        // {
        //     foreach (var item in redisOption.EndPoints)
        //     {
        //         redisCacheOptions.ConfigurationOptions.EndPoints.Add(item);
        //     }
        // }

        // var connection = RedisConnector.ConnectAsync(redisCacheOptions).ConfigureAwait(false).GetAwaiter().GetResult();
        // builder.AddRedisInstrumentation(connection, options =>
        // {
        //     options.FlushInterval = TimeSpan.FromSeconds(1);
        // });
        return builder;
    }
}