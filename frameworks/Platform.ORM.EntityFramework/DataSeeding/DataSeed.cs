using Microsoft.Extensions.DependencyInjection;

namespace Platform.ORM.EntityFramework.DataSeeding;

/// <summary>Gọi mọi <see cref="IDataSeeder"/> đã đăng ký, theo <see cref="IDataSeeder.Order"/>.</summary>
public static class DataSeed
{
    public static async Task RunAllAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        foreach (var seeder in services.GetServices<IDataSeeder>().OrderBy(s => s.Order))
            await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);
    }
}
