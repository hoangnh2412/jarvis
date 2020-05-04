using Infrastructure.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database.EntityFramework
{
    public static class ServiceCollectionExtension
    {
        public static void AddConfigEntityFramework(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
        }
    }
}
