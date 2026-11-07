using Microsoft.EntityFrameworkCore;

namespace Platform.Host.Postgresql.Extensions;

/// <summary>
/// Provider PostgreSQL. Connection string và đăng ký DbContext do host truyền vào.
/// Migration nằm trong assembly này.
/// </summary>
public static class PlatformHostPostgreSqlExtensions
{
    public static string MigrationsAssemblyName { get; } =
        typeof(PlatformHostPostgreSqlExtensions).Assembly.GetName().Name
        ?? "Platform.Host.Postgresql";

    /// <summary>Provider PostgreSQL, không đổi assembly migration. Connection string do host truyền vào.</summary>
    public static DbContextOptionsBuilder UsePlatformHostPostgreSql(
        this DbContextOptionsBuilder builder,
        string? connectionString) =>
        builder.UseNpgsql(RequireConnectionString(connectionString));

    private static string RequireConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Thiếu connection string.");

        return connectionString;
    }
}
