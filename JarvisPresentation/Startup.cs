using Infrastructure;
using Infrastructure.Caching.Redis;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;
using Jarvis.Core;
using Jarvis.Core.Database;
using Jarvis.Core.Database.SqlServer;
using Jarvis.Core.Models;
using JarvisPresentation.Domains;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddCors();
            services.AddControllers();

            // services.Configure<MvcJsonOptions>(options =>
            // {
            //     options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            //     options.SerializerSettings.ContractResolver = new DefaultContractResolver()
            //     {
            //         NamingStrategy = new CamelCaseNamingStrategy()
            //     };
            // });

            services.Configure<FileUploadOption>(Configuration.GetSection("FileUploadOption"));

            services.AddScoped<IStorageContext>(provider => provider.GetService<CoreDbContext>());
            services.AddDbContextPool<CoreDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Core"), sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__CoreMigrationHistory");
                });
            });
            services.AddScoped<ICoreUnitOfWork, CoreUnitOfWork>();
            
            services.AddConfigJarvisDefault<CoreDbContext>();
            services.AddConfigAuthentication();
            services.AddConfigAuthorization();

            services.AddRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "Jarvis";
            });

            services.AddScoped<IStorageContext>((serviceProvider) =>
            {
                return new TestDbContext(Configuration.GetConnectionString("Core"));
            });
            services.AddScoped<ITestUnitOfWork, TestUnitOfWork>();
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
