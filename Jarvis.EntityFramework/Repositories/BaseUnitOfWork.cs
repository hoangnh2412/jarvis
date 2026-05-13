using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.EntityFramework.Repositories;

public abstract class BaseUnitOfWork<T>(
    IServiceProvider services,
    IDbContextFactory<T> factory,
    IStorageContextTenantInitializer? tenantInitializer = null)
    : IUnitOfWork<T> where T : DbContext, IStorageContext
{
    private readonly IServiceProvider _services = services;
    private readonly IDbContextFactory<T> _factory = factory;
    private readonly IStorageContextTenantInitializer? _tenantInitializer = tenantInitializer;
    protected DbContext? StorageContext { get; set; }
    private bool _disposed;

    public IStorageContext GetDbContext()
    {
        if (StorageContext == null)
        {
            StorageContext = _factory.CreateDbContext();
            _tenantInitializer?.Initialize((IStorageContext)StorageContext);
        }

        return (IStorageContext)StorageContext;
    }

    public async Task<IStorageContext> GetDbContextAsync()
    {
        if (StorageContext == null)
        {
            StorageContext = await _factory.CreateDbContextAsync().ConfigureAwait(false);
            if (_tenantInitializer != null)
                await _tenantInitializer.InitializeAsync((IStorageContext)StorageContext).ConfigureAwait(false);
        }

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

        return await StorageContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        StorageContext?.Dispose();
        StorageContext = null;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        if (StorageContext != null)
            await StorageContext.DisposeAsync().ConfigureAwait(false);
        StorageContext = null;
        GC.SuppressFinalize(this);
    }
}