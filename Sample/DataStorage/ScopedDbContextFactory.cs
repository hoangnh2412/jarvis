using Microsoft.EntityFrameworkCore;

namespace Sample.DataStorage;

public class ScopedDbContextFactory : IDbContextFactory<SampleDbContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly DbContextOptions<SampleDbContext> _options;

    public ScopedDbContextFactory(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        DbContextOptions<SampleDbContext> options)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _options = options;
    }

    public SampleDbContext CreateDbContext()
    {
        var dbContext = new SampleDbContext(_options);

        var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();

        var name = nameof(SampleDbContext);
        if (httpContextAccessor.HttpContext != null && httpContextAccessor.HttpContext.Request.Headers.TryGetValue("region", out Microsoft.Extensions.Primitives.StringValues region))
            name = $"{name}.{region}";

        var connectionString = _configuration.GetConnectionString(name);

        var currentConnectionString = dbContext.Database.GetConnectionString();
        if (currentConnectionString == connectionString)
            return dbContext;

        dbContext.Database.SetConnectionString(connectionString);
        return dbContext;
    }
}