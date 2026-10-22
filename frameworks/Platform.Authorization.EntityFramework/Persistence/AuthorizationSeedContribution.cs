namespace Platform.Authorization.EntityFramework.Persistence;

/// <summary>Bộ đánh dấu DI cho một đóng góp seed lúc khởi động.</summary>
public sealed class AuthorizationSeedContribution(Action<AuthorizationSeedCatalog> configure)
{
    internal Action<AuthorizationSeedCatalog> Configure { get; } = configure ?? throw new ArgumentNullException(nameof(configure));
}
