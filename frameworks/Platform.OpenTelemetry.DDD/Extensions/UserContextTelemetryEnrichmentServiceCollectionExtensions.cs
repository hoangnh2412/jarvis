using Platform.DDD.Domain.Services;
using Platform.OpenTelemetry.Abstractions;
using Platform.OpenTelemetry.DDD;
using Platform.OpenTelemetry.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Platform.OpenTelemetry.DDD.Extensions;

public static class UserContextTelemetryEnrichmentServiceCollectionExtensions
{
    /// <summary>
    /// Registers default telemetry enrichers and a user/tenant
    /// <see cref="IEnrichmentSource"/> for <typeparamref name="TUser"/> / <typeparamref name="TTenant"/>.
    /// </summary>
    public static IServiceCollection AddUserContextTelemetryEnrichment<TUser, TTenant>(
        this IServiceCollection services)
        where TUser : class, ICurrentUserIdentity
        where TTenant : class, ICurrentTenantIdentity
    {
        services.AddTelemetryEnrichment();
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IEnrichmentSource, UserContextEnrichmentSource<TUser, TTenant>>());
        return services;
    }
}
