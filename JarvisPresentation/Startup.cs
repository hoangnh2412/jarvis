using System;
using System.Collections.Generic;
using Infrastructure.Caching.InMemory;
using Infrastructure.File;
using Infrastructure.File.Abstractions;
using Infrastructure.File.Minio;
using Infrastructure.Message.Rabbit;
using Jarvis.Core;
using Jarvis.Core.Database.SqlServer;
using Jarvis.Core.Events.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true);
            services.AddConfigJarvisDefault<CoreDbContext>();
            services.AddConfigSetting();
            services.AddConfigDefaultData();
            services.AddConfigAuthentication();
            services.AddConfigAuthorization();
            services.AddConfigPolicy();
            services.AddConfigNavigation();

            services.AddCoreDbContext(Configuration.GetConnectionString("App"));
            services.AddORM();

            // Caching
            services.AddInMemoryCache();
            // var redisOption = new RedisOption();
            // Configuration.GetSection("Redis").Bind(redisOption);
            // services.AddRedisCache(redisOption);
            // services.Configure<RedisOption>(Configuration.GetSection("Redis"));

            // MessageQueue
            var rabbitOption = new RabbitOption();
            Configuration.GetSection("RabbitMq").Bind(rabbitOption);
            services.AddRabbitMQ(rabbitOption);
            services.Configure<RabbitOption>(Configuration.GetSection("RabbitMq"));

            // ObjectStorage
            services.Configure<ObjectStorageOption>(Configuration.GetSection("Minio"));
            services.AddSingleton<IFileService, MinioService>();
            services.AddSingleton<IMinioHttpClient, MinioHttpClient>();

            // services.AddConfigHangfire(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseConfigUI("Jarvis.Core", "JarvisPresentation");
            app.UseConfigJarvisUI();

            app.UseConfigSwagger();
            app.UseRouting();

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.WithExposedHeaders("Content-Disposition");
            });

            // app.UseConfigHangfire();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseConfigMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
