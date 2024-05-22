using Jarvis.Application.MultiTenancy;
using Jarvis.Shared.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Sample.DataStorage;

public class ScopedDbContextFactory<T, TResolver> : IDbContextFactory<T> where T : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DbContextOptions<T> _options;

    public ScopedDbContextFactory(
        IHttpContextAccessor httpContextAccessor,
        DbContextOptions<T> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    public T CreateDbContext()
    {
        var dbContext = Activator.CreateInstance(typeof(T), _options) as DbContext;

        var resolver = _httpContextAccessor.HttpContext.RequestServices.GetService<ITenantConnectionStringResolver>(typeof(TResolver).Name);
        var connectionString = resolver.GetConnectionString(typeof(T).Name);

        var currentConnectionString = dbContext.Database.GetConnectionString();
        if (currentConnectionString == connectionString)
            return (T)dbContext;

        dbContext.Database.SetConnectionString(connectionString);
        return (T)dbContext;
    }
}