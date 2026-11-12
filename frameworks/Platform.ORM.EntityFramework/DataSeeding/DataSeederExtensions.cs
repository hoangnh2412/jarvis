using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Platform.ORM.EntityFramework.DataSeeding;

public static class DataSeederExtensions
{
    /// <summary>Đăng ký một bước seed EF (cho phép nhiều <see cref="IDataSeeder"/>).</summary>
    public static IServiceCollection AddDataSeeder<TSeeder>(this IServiceCollection services)
        where TSeeder : class, IDataSeeder
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TSeeder>());
        return services;
    }
}
