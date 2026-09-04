using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Jarvis.ORM.Dapper;

namespace UnitTest.Orm;

public class ConfigSqlConnectionFactoryTests
{
    [Fact]
    public async Task OpenAsync_UsesConfiguredConnectionStringAndFactory()
    {
        string? createdWith = null;
        using var host = OrmDapperTestHost.CreateHost(options =>
        {
            options.CreateConnection = connectionString =>
            {
                createdWith = connectionString;
                return new NpgsqlConnection(connectionString);
            };
        });
        var expected = OrmDapperTestHost.GetMasterConnectionString();

        var factory = host.Services.GetRequiredService<ISqlConnectionFactory>();
        await using var connection = (NpgsqlConnection)await factory.OpenAsync();

        Assert.Equal(ConnectionState.Open, connection.State);
        Assert.Equal(expected, createdWith);
    }

    [Fact]
    public async Task OpenAsync_Named_UsesExplicitConnectionStringName()
    {
        string? usedConnectionString = null;
        using var host = OrmDapperTestHost.CreateHost(options =>
        {
            options.CreateConnection = connectionString =>
            {
                usedConnectionString = connectionString;
                return new NpgsqlConnection(connectionString);
            };
        });

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var expectedTenant = configuration.GetConnectionString("TenantDbContext");

        var factory = host.Services.GetRequiredService<ISqlConnectionFactory>();
        await using var connection = (DbConnection)await factory.OpenAsync("TenantDbContext");

        Assert.Equal(expectedTenant, usedConnectionString);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task OpenAsync_DoesNotReopen_WhenConnectionAlreadyOpen()
    {
        var openCalls = 0;
        using var host = OrmDapperTestHost.CreateHost(options =>
        {
            options.CreateConnection = connectionString =>
            {
                var connection = new NpgsqlConnection(connectionString);
                connection.StateChange += (_, args) =>
                {
                    if (args.CurrentState == ConnectionState.Open)
                        openCalls++;
                };
                connection.Open();
                return connection;
            };
        });

        var factory = host.Services.GetRequiredService<ISqlConnectionFactory>();
        await using var connection = (DbConnection)await factory.OpenAsync();

        Assert.Equal(1, openCalls);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task OpenAsync_Throws_WhenCreateConnectionMissing()
    {
        using var host = OrmDapperTestHost.CreateHost(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=test",
            },
            options => options.CreateConnection = null);

        var factory = host.Services.GetRequiredService<ISqlConnectionFactory>();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => factory.OpenAsync());
        Assert.Contains("CreateConnection", ex.Message);
    }

    [Fact]
    public async Task OpenAsync_Throws_WhenConnectionStringMissing()
    {
        using var host = OrmDapperTestHost.CreateHost(
            new Dictionary<string, string?>(),
            options => options.ConnectionStringName = "MissingDb");

        var factory = host.Services.GetRequiredService<ISqlConnectionFactory>();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => factory.OpenAsync());
        Assert.Contains("MissingDb", ex.Message);
    }

    [Fact]
    public async Task OpenAsync_Throws_WhenCreateConnectionReturnsNull()
    {
        using var host = OrmDapperTestHost.CreateHost(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=test",
            },
            options => options.CreateConnection = _ => null!);

        var factory = host.Services.GetRequiredService<ISqlConnectionFactory>();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => factory.OpenAsync());
        Assert.Contains("returned null", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task OpenAsync_Named_Throws_WhenConnectionStringNameInvalid(string? name)
    {
        using var host = OrmDapperTestHost.CreateHost(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=test",
            });

        var factory = host.Services.GetRequiredService<ISqlConnectionFactory>();
        await Assert.ThrowsAnyAsync<ArgumentException>(() => factory.OpenAsync(name!));
    }

    [Fact]
    public async Task OpenAsync_WithPostgres_CanQueryViaDapper()
    {
        using var host = OrmDapperTestHost.CreateHost();
        var factory = host.Services.GetRequiredService<ISqlConnectionFactory>();
        await using var connection = (DbConnection)await factory.OpenAsync();

        var value = await Dapper.SqlMapper.ExecuteScalarAsync<int>(
            connection,
            new Dapper.CommandDefinition("SELECT 1"));

        Assert.Equal(1, value);
    }
}
