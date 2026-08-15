using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sample.Services;

namespace UnitTest.Orm;

public class SampleDapperReadSampleTests
{
    [Fact]
    public async Task PingAsync_ReturnsOne_WhenPostgresConnectionWorks()
    {
        using var host = OrmDapperTestHost.CreateHost();
        var sample = ActivatorUtilities.CreateInstance<SampleDapperReadSample>(host.Services);
        var result = await sample.PingAsync();
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task PingAsync_UsesFactory_AndDisposesConnection()
    {
        var closed = false;
        var connectionString = OrmDapperTestHost.GetMasterConnectionString();
        var factory = new DelegateSqlConnectionFactory(() =>
        {
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            connection.StateChange += (_, args) =>
            {
                if (args.CurrentState == ConnectionState.Closed)
                    closed = true;
            };
            return connection;
        });

        var sample = new SampleDapperReadSample(factory);
        var result = await sample.PingAsync();

        Assert.Equal(1, result);
        Assert.True(closed);
    }

    [Fact]
    public async Task PingAsync_PropagatesFactoryErrors()
    {
        var factory = new DelegateSqlConnectionFactory(
            () => throw new InvalidOperationException("boom"));

        var sample = new SampleDapperReadSample(factory);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sample.PingAsync());
        Assert.Equal("boom", ex.Message);
    }

    private sealed class DelegateSqlConnectionFactory(Func<System.Data.IDbConnection> open)
        : Jarvis.ORM.Dapper.ISqlConnectionFactory
    {
        public Task<System.Data.IDbConnection> OpenAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(open());

        public Task<System.Data.IDbConnection> OpenAsync(
            string connectionStringName,
            CancellationToken cancellationToken = default)
            => Task.FromResult(open());
    }
}
