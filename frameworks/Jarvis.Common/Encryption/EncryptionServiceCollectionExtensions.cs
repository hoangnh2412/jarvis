using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Common.Encryption;

/// <summary>
/// Đăng ký <see cref="EncryptionOptions"/> (section <c>Encryption</c>) vào DI — dùng chung mọi module.
/// </summary>
public static class EncryptionServiceCollectionExtensions
{
    /// <summary>
    /// Bind <see cref="EncryptionOptions"/> từ cấu hình. Gọi nhiều lần an toàn (chỉ đăng ký một lần).
    /// </summary>
    public static IServiceCollection AddEncryptionOptions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Marker tránh BindConfiguration lặp khi nhiều module cùng gọi.
        if (services.Any(d => d.ServiceType == typeof(EncryptionOptionsRegistration)))
            return services;

        services.AddSingleton<EncryptionOptionsRegistration>();
        services
            .AddOptions<EncryptionOptions>()
            .BindConfiguration(EncryptionOptions.SectionName);

        return services;
    }

    /// <summary>Marker nội bộ — có trong DI nghĩa là options đã được bind.</summary>
    private sealed class EncryptionOptionsRegistration;
}
