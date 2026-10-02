// Platform.BlobStoring — Shared DI helpers for blob provider registration.
using Microsoft.Extensions.DependencyInjection;

namespace Platform.BlobStoring.Hosting;

public static class BlobStoringServiceCollectionExtensions
{
    public static BlobStoringProviderRegistry GetOrAddProviderRegistry(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == typeof(BlobStoringProviderRegistry) && descriptor.ImplementationInstance is BlobStoringProviderRegistry registry)
                return registry;
        }

        var created = new BlobStoringProviderRegistry();
        services.AddSingleton(created);
        return created;
    }
}
