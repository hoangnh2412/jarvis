using System;
using System.Collections.Generic;
using Infrastructure.Abstractions.Events;
using Infrastructure.Caching.Redis;
using Infrastructure.Database;
using Infrastructure.Database.Abstractions;
using Infrastructure.File.Abstractions;
using Infrastructure.File.Minio;
using Jarvis.Core;
using Jarvis.Core.Database.SqlServer;
using JarvisPresentation.Domains;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JarvisPresentation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddConfigJarvisDefault<CoreDbContext>();
            services.AddConfigAuthentication();
            services.AddConfigAuthorization();

            services.AddRedis(Configuration.GetSection("Redis"));
            services.AddCoreDbContext(Configuration.GetConnectionString("Core"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseConfigJarvisDefault();
        }
    }
}
