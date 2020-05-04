using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public abstract class CoreInitializer : IModuleInitializer
    {
        public int Piority => 2000;

        public abstract void Configure(IApplicationBuilder app);

        public abstract void ConfigureServices(IServiceCollection services);
    }
}
