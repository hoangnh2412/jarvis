using Jarvis.Core;
using Jarvis.Core.Database.SqlServer;
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddConfigJarvisDefault<CoreDbContext>();
            services.AddConfigAuthentication();
            services.AddConfigAuthorization();

            services.AddRedis(Configuration.GetSection("Redis"));
            services.AddCoreDbContext(Configuration.GetConnectionString("Core"));
            services.AddORM();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseConfigUI("Jarvis.Core", "JarvisPresentation");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseConfigSwagger();
            app.UseConfigMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
