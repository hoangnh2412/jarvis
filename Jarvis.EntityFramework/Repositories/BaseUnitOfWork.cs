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
    protected DbContext? StorageContext { get; set; }

    public IStorageContext GetDbContext()
    {
        if (StorageContext == null)
            StorageContext = Task.Run(() => _factory.CreateDbContextAsync()).GetAwaiter().GetResult();

        return (IStorageContext)StorageContext;
    }

    public async Task<IStorageContext> GetDbContextAsync()
    {
        if (StorageContext == null)
            StorageContext = await _factory.CreateDbContextAsync();

        return (IStorageContext)StorageContext;
    }

    public TRepository GetRepository<TRepository>() where TRepository : IRepository
    {
        var repo = (TRepository)_services.GetRequiredService(typeof(TRepository)) ?? throw new NullReferenceException($"Service name {typeof(TRepository).Name} can't resolve");
        repo.SetStorageContext(GetDbContext());
        return repo;
    }

    public async Task<int> CommitAsync()
    {
        if (StorageContext == null) throw new NullReferenceException(nameof(StorageContext));

        return await StorageContext.SaveChangesAsync();
    }
}