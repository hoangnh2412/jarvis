using Microsoft.EntityFrameworkCore;
using Platform.Authorization;
using Platform.Authorization.EntityFramework;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.ORM.EntityFramework.DataSeeding;
using Sample.Persistence;

namespace Sample.AuthorizationDemo;

/// <summary>
/// Gắn Platform.Authorization (RBAC + ABAC) vào Sample; identity nằm trên <see cref="SampleTenantDbContext"/> (schema <c>identity</c>).
/// </summary>
public static class SampleAuthorizationExtensions
{
    public static WebApplicationBuilder AddSampleAuthorization(this WebApplicationBuilder builder)
    {
        builder.AddPlatformAuthorization();
        builder.AddPlatformAuthorizationEntityFramework();
        builder.AddPlatformAuthorizationOperands(SampleAuthorizationDemoData.ConfigureOperands);
        builder.AddPlatformAuthorizationSeed(SampleAuthorizationDemoData.ConfigureSeed);

        builder.Services.AddScoped<IdentityDbContextBase>(sp => sp.GetRequiredService<SampleTenantDbContext>());
        // Bật khi cần role claim / user / expense demo (sau catalog seed Order 0).
        // builder.Services.AddDataSeeder<SampleAuthorizationDemoDatabaseSeeder>();

        return builder;
    }

    /// <summary>Seed platform + demo RBAC/ABAC trên database tenant (sau migrate).</summary>
    public static async Task SeedSampleAuthorizationAsync(this WebApplication app, bool recreate = false)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            await SeedAsync(scope.ServiceProvider, recreate);
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Bỏ qua seed demo phân quyền: không chuẩn bị được database tenant.");
        }
    }

    public static async Task SeedAsync(IServiceProvider services, bool recreate, CancellationToken cancellationToken = default)
    {
        var context = services.GetRequiredService<SampleTenantDbContext>();
        if (recreate)
        {
            await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }

        await DataSeed.RunAllAsync(services, cancellationToken).ConfigureAwait(false);
    }
}
