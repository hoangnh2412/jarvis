using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Jarvis.ORM.Dapper;

namespace UnitTest.Orm;

public class AddOrmDapperTests
{
    [Fact]
    public void AddOrmDapper_Registers_ISqlConnectionFactory_AsSingleton()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddOrmDapper(options =>
        {
            options.ConnectionStringName = "MasterDbContext";
            options.CreateConnection = cs => new NpgsqlConnection(cs);
        });

        using var host = builder.Build();
        var first = host.Services.GetRequiredService<ISqlConnectionFactory>();
        var second = host.Services.GetRequiredService<ISqlConnectionFactory>();

        Assert.Same(first, second);
        Assert.IsType<ConfigSqlConnectionFactory>(first);
    }

    [Fact]
    public void AddOrmDapper_TryAdd_DoesNotReplaceExistingRegistration()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<ISqlConnectionFactory, AlternateSqlConnectionFactory>();
        builder.AddOrmDapper(options => options.CreateConnection = cs => new NpgsqlConnection(cs));

        using var host = builder.Build();
        Assert.IsType<AlternateSqlConnectionFactory>(host.Services.GetRequiredService<ISqlConnectionFactory>());
    }

    [Fact]
    public void AddOrmDapper_AppliesOptions()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddOrmDapper(options =>
        {
            options.ConnectionStringName = "TenantDbContext";
            options.CreateConnection = cs => new NpgsqlConnection(cs);
        });

        using var host = builder.Build();
        var options = host.Services.GetRequiredService<IOptions<OrmDapperOptions>>().Value;

        Assert.Equal("TenantDbContext", options.ConnectionStringName);
        Assert.NotNull(options.CreateConnection);
    }

    [Fact]
    public void AddOrmDapper_Throws_WhenBuilderNull()
    {
        IHostApplicationBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() => builder!.AddOrmDapper());
    }

    private sealed class AlternateSqlConnectionFactory : ISqlConnectionFactory
    {
        public Task<System.Data.IDbConnection> OpenAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<System.Data.IDbConnection> OpenAsync(string connectionStringName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
