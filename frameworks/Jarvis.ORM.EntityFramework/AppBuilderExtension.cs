using Jarvis.DDD.Domain.Repositories;
using Jarvis.ORM.EntityFramework.DataStorages;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.ORM.EntityFramework;

public static class AppBuilderExtension
{
    /// <summary>
    /// Chạy migrate qua <see cref="IUnitOfWork"/> (factory từ DI).
    /// Phù hợp master / DB dùng chung — không bị ghi đè connection string khi mở kết nối.
    /// </summary>
    /// <typeparam name="T">Kiểu UnitOfWork cần migrate.</typeparam>
    /// <param name="builder">Application builder.</param>
    public static IApplicationBuilder EnsureMigrateDb<T>(this IApplicationBuilder builder) where T : IUnitOfWork
    {
        var configuration = builder.ApplicationServices.GetRequiredService<IConfiguration>();
        var enableAutoMigrate = configuration.GetValue<bool>("ConnectionStrings:AutoMigrate");
        if (!enableAutoMigrate)
            return builder;

        using (var scope = builder.ApplicationServices.CreateScope())
        using (TenantScopedContextValidation.BeginSuppressScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<T>();
            var dbContext = uow.GetDbContextAsync().GetAwaiter().GetResult() as DbContext;
            dbContext?.Database.MigrateAsync().GetAwaiter().GetResult();
        }

        return builder;
    }

    /// <summary>
    /// Migrate template tenant DbContext với connection string cố định, <b>không</b> qua
    /// Multitenancy.EF <c>TenantDbConnectionInterceptor</c>, để provider có thể tạo database
    /// (ví dụ PostgreSQL kết nối <c>postgres</c> rồi <c>CREATE DATABASE</c>).
    /// </summary>
    /// <typeparam name="TDbContext">Kiểu context cụ thể, có ctor public nhận <see cref="DbContextOptions{TContext}"/>.</typeparam>
    /// <param name="builder">Application builder.</param>
    /// <param name="configure">Cấu hình provider từ config, ví dụ <c>(cfg, o) => o.UseNpgsql(cfg.GetConnectionString("TenantDbContext"))</c>.</param>
    public static IApplicationBuilder EnsureMigrateTemplateDb<TDbContext>(
        this IApplicationBuilder builder,
        Action<IConfiguration, DbContextOptionsBuilder<TDbContext>> configure)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(configure);

        var configuration = builder.ApplicationServices.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue<bool>("ConnectionStrings:AutoMigrate"))
            return builder;

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        configure(configuration, optionsBuilder);

        var dbContext = (TDbContext?)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)
            ?? throw new InvalidOperationException(
                $"Could not create '{typeof(TDbContext).Name}'. Expected a public constructor accepting DbContextOptions<{typeof(TDbContext).Name}>.");

        using (dbContext)
        using (TenantScopedContextValidation.BeginSuppressScope())
        {
            dbContext.Database.MigrateAsync().GetAwaiter().GetResult();
        }

        return builder;
    }
}
