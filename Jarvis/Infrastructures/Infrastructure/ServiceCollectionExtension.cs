using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Infrastructure
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddJarvis(this IServiceCollection services)
        {
            services.AddSingleton<IModuleManager, ModuleManager>();

            var provider = services.BuildServiceProvider();
            var configuration = provider.GetService<IConfiguration>();
            services.Configure<ApplicationInfo>(configuration.GetSection("ApplicationInfo"));
            return services;
        }

        public static IServiceCollection AddModule<T>(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IModuleInitializer), typeof(T));
            return services;
        }

        public static void BuildJarvis(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var modules = provider.GetServices<IModuleInitializer>();
            modules = modules.OrderBy(x => x.Piority).ToList();

            foreach (var item in modules)
            {
                item.ConfigureServices(services);
            }
        }

        public static void UseJarvis(this IApplicationBuilder app)
        {
            var modules = app.ApplicationServices.GetServices<IModuleInitializer>();
            modules = modules.OrderBy(x => x.Piority);
            foreach (var module in modules)
            {
                module.Configure(app);
            }
        }
    }
}
