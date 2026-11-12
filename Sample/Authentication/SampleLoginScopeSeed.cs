using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Repositories;
using Sample.Persistence;

namespace Sample.Authentication;

/// <summary>Org mặc định của tenant dev để ô chọn trên màn đăng nhập có một bản ghi.</summary>
public static class SampleLoginScopeSeed
{
    public static readonly Guid DefaultOrgId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public const string DefaultOrgCode = "default";

    public static async Task EnsureAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var tenants = scope.ServiceProvider.GetRequiredService<ISampleTenantUnitOfWork>();
            await tenants.SwitchDbContextAsync(SampleAuthTenants.DevTenantId, cancellationToken).ConfigureAwait(false);
            var repo = await tenants.GetRepositoryAsync<IRepository<Sample.Entities.Org>>(cancellationToken).ConfigureAwait(false);
            var exists = await repo.GetQuery()
                .AnyAsync(row => row.Code == DefaultOrgCode, cancellationToken)
                .ConfigureAwait(false);
            if (exists)
                return;

            await repo.InsertAsync(new Sample.Entities.Org
            {
                Id = DefaultOrgId,
                TenantId = SampleAuthTenants.DevTenantId,
                Code = DefaultOrgCode,
                Name = "Sample",
                IsActive = true,
            }, cancellationToken).ConfigureAwait(false);
            await tenants.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Bỏ qua seed org mặc định cho màn đăng nhập.");
        }
    }
}
