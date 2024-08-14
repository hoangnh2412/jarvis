using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.EntityFramework.Repositories;

public abstract class BaseUnitOfWork<T>(
    IServiceProvider services,
    IDbContextFactory<T> factory)
    : IUnitOfWork<T> where T : DbContext, IStorageContext
{
    private readonly IServiceProvider _services = services;
    private readonly IDbContextFactory<T> _factory = factory;

    public IStorageContext GetDbContext() => _factory.CreateDbContext() ?? throw new NullReferenceException($"Can't create DbContext {typeof(T).Name}");
    private DbContext GetDbContextInternal() => (DbContext)GetDbContext();

    public IStorageContext GetDbContext<TResolver>(string name) where TResolver : ITenantConnectionStringResolver
    {
        var resolver = _services.GetRequiredKeyedService<TResolver>(typeof(TResolver).Name);

        var connectionString = resolver.Resolve(name);

        GetDbContextInternal().Database.SetConnectionString(connectionString);
        return GetDbContext();
    }

    public string GetConnectionString()
    {
        return GetDbContextInternal().Database.GetDbConnection().ConnectionString;
    }

    public TRepository GetRepository<TRepository>() where TRepository : IRepository
    {
        var repo = (TRepository)_services.GetRequiredService(typeof(TRepository));
        if (repo == null)
            throw new NullReferenceException($"Service name {typeof(TRepository).Name} can't resolve");

        repo.SetStorageContext(GetDbContext());
        return repo;
    }

    public async Task<int> CommitAsync()
    {
        return await GetDbContextInternal().SaveChangesAsync();
    }
}