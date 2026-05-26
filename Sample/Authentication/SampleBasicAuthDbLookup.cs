using Jarvis.Authentication.Basic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sample.Persistence;

namespace Sample.Authentication;

/// <summary>
/// Tra credential Basic Auth từ <see cref="MasterDbContext"/> — dùng làm delegate cho <c>AddCoreBasic</c>.
/// </summary>
public static class SampleBasicAuthDbLookup
{
    private static IServiceScopeFactory? _scopeFactory;

    /// <summary>Gán scope factory sau <c>builder.Build()</c> — delegate lookup cần resolve scoped DbContext.</summary>
    public static void Configure(IServiceProvider services) =>
        _scopeFactory = services.GetRequiredService<IServiceScopeFactory>();

    /// <summary>Delegate truyền vào <c>AddCoreBasic(configuration, lookup)</c>.</summary>
    public static BasicCredentialLookupAsync Lookup => LookupAsync;

    private static async Task<BasicUserCredential?> LookupAsync(
        string schemeName,
        string username,
        CancellationToken cancellationToken)
    {
        if (_scopeFactory is null)
            throw new InvalidOperationException("Call SampleBasicAuthDbLookup.Configure(app.Services) after builder.Build().");

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MasterDbContext>();

        var user = await db.BasicAuthUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user is null)
            return null;

        return new BasicUserCredential
        {
            Password = user.Password,
            Roles = user.Roles
        };
    }
}
