// Jarvis.BlobStoring — Registered blob providers and default key resolution (extensible without editing core options).
namespace Jarvis.BlobStoring.Hosting;

/// <summary>
/// Providers register here from satellite packages (<c>UseMinIO</c>, <c>UseAwsS3</c>, …).
/// Higher <see cref="BlobStoringProviderRegistration.AutoSelectPriority"/> wins when <c>DefaultProvider</c> is empty.
/// </summary>
public sealed class BlobStoringProviderRegistry
{
    private readonly List<BlobStoringProviderRegistration> _providers = [];

    public void Register(string providerKey, int autoSelectPriority)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        _providers.RemoveAll(p => p.ProviderKey == providerKey);
        _providers.Add(new BlobStoringProviderRegistration(providerKey, autoSelectPriority));
    }

    public string ResolveDefaultProviderKey(string? defaultProvider)
    {
        if (!string.IsNullOrWhiteSpace(defaultProvider))
            return defaultProvider;

        if (_providers.Count == 0)
            return nameof(BlobStoringType.FileSystem);

        return _providers
            .OrderByDescending(p => p.AutoSelectPriority)
            .First()
            .ProviderKey;
    }
}

public readonly record struct BlobStoringProviderRegistration(string ProviderKey, int AutoSelectPriority);
