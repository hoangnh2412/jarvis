using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Jarvis.ORM.Dapper;

namespace UnitTest.Orm;

internal static class OrmDapperTestHost
{
    public const string DefaultConnectionName = "MasterDbContext";

    /// <summary>
    /// Builds a host with <see cref="AddOrmDapper"/> wired to Sample <c>appsettings.json</c> + Npgsql.
    /// </summary>
    public static IHost CreateHost(
        Action<OrmDapperOptions>? configure = null,
        string connectionStringName = DefaultConnectionName)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddJsonFile(GetSampleAppSettingsPath(), optional: false, reloadOnChange: false);
        builder.AddOrmDapper(options =>
        {
            options.ConnectionStringName = connectionStringName;
            options.CreateConnection = connectionString => new NpgsqlConnection(connectionString);
            configure?.Invoke(options);
        });

        return builder.Build();
    }

    /// <summary>
    /// Builds a host with in-memory configuration (error-path / named-string unit tests).
    /// </summary>
    public static IHost CreateHost(
        IDictionary<string, string?> configuration,
        Action<OrmDapperOptions>? configure = null)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(configuration);
        builder.AddOrmDapper(options =>
        {
            options.CreateConnection = connectionString => new NpgsqlConnection(connectionString);
            configure?.Invoke(options);
        });
        return builder.Build();
    }

    public static string GetMasterConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(GetSampleAppSettingsPath(), optional: false, reloadOnChange: false)
            .Build();

        return configuration.GetConnectionString(DefaultConnectionName)
            ?? throw new InvalidOperationException(
                $"Connection string '{DefaultConnectionName}' was not found in Sample/appsettings.json.");
    }

    public static string GetSampleAppSettingsPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Jarvis.sln")))
            dir = dir.Parent;

        var repoRoot = dir?.FullName
            ?? throw new InvalidOperationException("Could not find repo root (Jarvis.sln).");

        return Path.Combine(repoRoot, "Sample", "appsettings.json");
    }
}
