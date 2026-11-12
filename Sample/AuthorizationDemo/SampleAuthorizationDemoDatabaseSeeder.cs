using Platform.ORM.EntityFramework.DataSeeding;
using Sample.Persistence;

namespace Sample.AuthorizationDemo;

/// <summary>Role claims, user demo và expense mẫu — chạy sau catalog seed (Order 0).</summary>
public sealed class SampleAuthorizationDemoDatabaseSeeder(SampleTenantDbContext context) : IDataSeeder
{
    public int Order => 100;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SampleAuthorizationDemoData.EnsureDemoDataAsync(context, cancellationToken).ConfigureAwait(false);
        await SampleAuthorizationDemoData.EnsureExpensesAsync(context, cancellationToken).ConfigureAwait(false);
    }
}
