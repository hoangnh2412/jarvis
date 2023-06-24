using System;
using Infrastructure.Database;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace JarvisPresentation
{
    public static class ServiceCollectionExtension
    {
        public static void AddORM(this IServiceCollection services)
        {
            services.AddScoped<Func<string, IStorageContext>>(sp => name => (IStorageContext)sp.GetService(InstanceNames.DbContexts[name]));
            services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
        }
    }
}