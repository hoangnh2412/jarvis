using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public abstract class InfrastructureInitializer : IModuleInitializer
    {
        public int Piority => 1000;

        public abstract void Configure(IApplicationBuilder app);

        public abstract void ConfigureServices(IServiceCollection services);
    }
}
