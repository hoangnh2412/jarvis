using Microsoft.EntityFrameworkCore;
using Npgsql;
using Platform.Authorization;
using Platform.Authorization.EntityFramework;
using Platform.Authorization.EntityFramework.Persistence;

namespace Sample.AuthorizationDemo;

/// <summary>
/// Gắn Platform.Authorization (RBAC + ABAC) vào Sample để kiểm thử trực tiếp qua <c>api/_authz-demo</c>.
/// </summary>
public static class SampleAuthorizationExtensions
{
    public const string ConnectionStringName = "AuthorizationDbContext";
    private const string DefaultDatabase = "SampleAuthorization";

    public static WebApplicationBuilder AddSampleAuthorization(this WebApplicationBuilder builder)
    {
        builder.AddPlatformAuthorization();
        builder.AddPlatformAuthorizationEntityFramework();
        builder.AddPlatformAuthorizationOperands(SampleAuthorizationDemoData.ConfigureOperands);
        builder.AddPlatformAuthorizationSeed(SampleAuthorizationDemoData.ConfigureSeed);

        var connectionString = ResolveConnectionString(builder.Configuration);
        builder.Services.AddDbContext<SampleAuthorizationDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddScoped<IdentityDbContextBase>(sp => sp.GetRequiredService<SampleAuthorizationDbContext>());

        return builder;
    }

    /// <summary>
    /// Tạo schema (nếu chưa có), seed mặc định platform + đóng góp của Sample, rồi thêm dữ liệu demo.
    /// </summary>
    public static async Task SeedSampleAuthorizationAsync(this WebApplication app, bool recreate = false)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            await SeedAsync(scope.ServiceProvider, recreate);
        }
        catch (Exception ex)
        {
            // Demo không được làm hỏng khởi động Sample; gọi POST api/_authz-demo/reset sau khi sửa kết nối.
            app.Logger.LogWarning(ex, "Bỏ qua seed demo phân quyền: không chuẩn bị được database {Name}.", ConnectionStringName);
        }
    }

    public static async Task SeedAsync(IServiceProvider services, bool recreate, CancellationToken cancellationToken = default)
    {
        var context = services.GetRequiredService<SampleAuthorizationDbContext>();
        if (recreate)
            await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        await services.GetRequiredService<AuthorizationDataSeeder>().SeedAsync(cancellationToken);
        await SampleAuthorizationDemoData.EnsureDemoDataAsync(context, cancellationToken);
        await SampleAuthorizationDemoData.EnsureExpensesAsync(context, cancellationToken);
    }

    /// <summary>
    /// Ưu tiên <c>ConnectionStrings:AuthorizationDbContext</c>; nếu không có thì dùng server của
    /// <c>MasterDbContext</c> với database <c>SampleAuthorization</c>.
    /// </summary>
    private static string ResolveConnectionString(IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString(ConnectionStringName);
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        var master = configuration.GetConnectionString("MasterDbContext")
            ?? throw new InvalidOperationException($"Thiếu ConnectionStrings:{ConnectionStringName} hoặc ConnectionStrings:MasterDbContext.");
        return new NpgsqlConnectionStringBuilder(master) { Database = DefaultDatabase }.ConnectionString;
    }
}
