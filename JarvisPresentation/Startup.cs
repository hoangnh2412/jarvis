using Infrastructure.Abstractions;
using Infrastructure.Abstractions.Events;
using Infrastructure.Caching.Redis;
using Infrastructure.Database.Abstractions;
using Infrastructure.File.Abstractions;
using Infrastructure.File.Minio;
using Jarvis.Core;
using Jarvis.Core.Database;
using Jarvis.Core.Database.SqlServer;
using Jarvis.Core.Events;
using Jarvis.Core.Models;
using JarvisPresentation.Domains;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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

            services.AddRedisCache(options =>
            {
                options.InstanceName = "Jarvis";
                options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions {
                    
                }
            });

            services.AddScoped<IStorageContext>((serviceProvider) =>
            {
                return new TestDbContext(Configuration.GetConnectionString("Core"));
            });
            services.AddScoped<ITestUnitOfWork, TestUnitOfWork>();

            //Minio
            services.Configure<MinioOptions>((options) =>
            {
                options.AccessKey = "Q3AM3UQ867SPQQA43P2F";
                options.BucketName = "vnis";
                options.Endpoint = "192.168.1.6:5000";
                options.Region = "us-east-1";
                options.SecretKey = "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG";
            });
            services.AddSingleton<IFileService, MinioService>((serviceProvider) =>
            {
                var options = serviceProvider.GetService<IOptions<MinioOptions>>();
                var logger = serviceProvider.GetService<ILogger<MinioService>>();
                return new MinioService(
                    minioOptions: options,
                    minio: new Minio.MinioClient(
                        options.Value.Endpoint,
                        options.Value.AccessKey,
                        options.Value.SecretKey,
                        options.Value.Region
                    ),
                    logger: logger
                );
            });

            services.AddSingleton<IEventFactory, EventFactory>();
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
