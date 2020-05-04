using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public abstract class AssemblyInitializer : IModuleInitializer
    {
        public int Piority => 3000;

        public abstract void Configure(IApplicationBuilder app);

        public abstract void ConfigureServices(IServiceCollection services);
    }
}
