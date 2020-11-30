using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Jarvis.Core.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Core
{
    public static class ApplicationBuilderExtension
    {
        public static void UseConfigJarvisDefault(this IApplicationBuilder app, string jarvisPath, params string[] modules)
        {
            app.UseHsts();
            app.UseHttpsRedirection();

            app.UseConfigUI(jarvisPath, modules);

            app.UseConfigJarvisUI();
            app.UseRouting();

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.WithExposedHeaders("Content-Disposition");
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseConfigSwagger();
            app.UseConfigMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static void UseConfigMiddleware(this IApplicationBuilder app)
        {
            app.UseWhen(httpContext =>
            {
                if (httpContext.Request.Path.ToString().StartsWith("/swagger"))
                    return false;

                if (!httpContext.Request.Headers.ContainsKey("Envelope"))
                    return true;

                return false;
            }, appBuilder =>
            {
                appBuilder.UseMiddleware<ResponseMiddleware>();
            });

            app.UseWhen(httpContext => !httpContext.Request.Path.ToString().StartsWith("/swagger"), appBuilder =>
            {
                appBuilder.UseMiddleware<AuthMiddlerware>();
                appBuilder.UseMiddleware<ResponseMiddleware>();
                appBuilder.UseMiddleware<LoggingMiddleware>();
            });
        }

        public static void UseConfigUI(this IApplicationBuilder app, string jarvisPath, params string[] modules)
        {
            var env = app.ApplicationServices.GetService<IWebHostEnvironment>();

            //Môi trường DEV: Scan toàn bộ project, load tất cả folder wwwroot để chạy FE
            //Môi trường PROD: chạy FE trong folder wwwroot
            if (env.IsDevelopment())
            {
                app.UseDefaultFiles();

                var index = env.ContentRootPath.LastIndexOf(env.ApplicationName);
                var solutionPath = env.ContentRootPath.Substring(0, index - 1);
                var wwwroots = Directory.GetDirectories(solutionPath, "wwwroot", SearchOption.AllDirectories)
                    .Where(x => !x.Contains("Release"))
                    .Select(x => new
                    {
                        FullPath = x,
                        Path = x.Substring(index, x.Length - index)
                    })
                    .ToList();

                var name = Assembly.GetEntryAssembly().GetName().Name;
                var paths = new List<string>();
                paths.AddRange(wwwroots.Where(x => x.Path.Contains("Jarvis.Core")).Select(x => x.FullPath));
                foreach (var module in modules)
                {
                    paths.AddRange(wwwroots.Where(x => x.Path.StartsWith(module)).Select(x => x.FullPath));
                }
                paths.AddRange(wwwroots.Where(x => x.Path == Path.Combine(name, "wwwroot")).Select(x => x.FullPath));

                if (!string.IsNullOrWhiteSpace(jarvisPath))
                {
                    var jarvisWwwroots = Directory.GetDirectories(jarvisPath, "wwwroot", SearchOption.AllDirectories)
                        .Where(x => !x.Contains("Release"))
                        .Select(x => new
                        {
                            FullPath = x,
                            Path = x.Substring(jarvisPath.Length, x.Length - jarvisPath.Length)
                        })
                        .ToList();
                    paths.AddRange(jarvisWwwroots.Where(x => x.Path.Contains("Jarvis.Core")).Select(x => x.FullPath));
                }

                foreach (var path in paths)
                {
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(path),
                    });
                }
            }
            else
            {
                app.UseDefaultFiles();
                app.UseStaticFiles();
            }
        }

        public static void UseConfigSwagger(this IApplicationBuilder app, Dictionary<string, string> endpoints = null, string prefix = null)
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = prefix + "/swagger/{documentName}/swagger.json";
                // options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                // {
                //     swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{prefix}" } };
                // });
            });
            app.UseSwaggerUI(options =>
            {
                if (endpoints == null)
                {
                    options.SwaggerEndpoint("/swagger/v0/swagger.json", $"{Assembly.GetEntryAssembly().GetName().Name} API");
                    // options.RoutePrefix = string.Empty;
                }
                else
                {
                    foreach (var endpoint in endpoints)
                    {
                        options.SwaggerEndpoint(endpoint.Key, endpoint.Value);
                    }
                    // options.RoutePrefix = prefix;
                }
            });
        }

        public static void UseConfigJarvisUI(this IApplicationBuilder app)
        {
            var type = typeof(ApplicationBuilderExtension);
            var assembly = type.Assembly;
            var space = $"{type.Namespace}.wwwroot".Length;
            var extensions = new List<string> { ".min.css", ".min.js", ".module.js", ".component.js", ".controller.js", ".route.js", ".service.js", ".config.js", ".directive.js", ".filter.js", ".template.html" };

            var resources = assembly.GetManifestResourceNames();
            var files = new List<KeyValuePair<string, string>>();
            foreach (var resource in resources)
            {
                string fileName;
                var file = resource[space..];
                var index = extensions.FindIndex(x => file.Contains(x));
                if (index > -1) //Các file SPA
                {
                    var extension = extensions[index];
                    var path = file.Substring(0, file.Length - extension.Length);
                    path = path.Replace('.', '/').Replace('_', '-');
                    fileName = $"{path}{extension}";
                }
                else //Các file thường
                {
                    var last = file.LastIndexOf('.');
                    var path = file.Substring(0, last);
                    var extension = file[last..];
                    path = path.Replace('.', '/').Replace('_', '-');
                    fileName = $"{path}{extension}";
                }

                using var contentStream = assembly.GetManifestResourceStream(resource);
                using var reader = new StreamReader(contentStream);
                files.Add(new KeyValuePair<string, string>($"{fileName}", reader.ReadToEnd()));
            }

            var fonts = new List<string> { ".woff2", ".woff", ".ttf" };
            foreach (var file in files)
            {
                var path = file.Key;
                path = path.Replace("/app/", "/app/jarvis/");
                app.Map(path, builder =>
                {
                    builder.Run(async context =>
                    {
                        if (path.Contains(".woff"))
                            context.Response.ContentType = "application/font-woff";

                        if (path.Contains(".woff2"))
                            context.Response.ContentType = "application/font-woff2";

                        if (path.Contains(".ttf"))
                            context.Response.ContentType = "application/font-ttf";

                        await context.Response.WriteAsync(file.Value);
                    });
                });
            }
        }
    }
}