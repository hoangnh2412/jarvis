using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Infrastructure.Database.EntityFramework;
using System.Collections.Generic;
using Infrastructure.Abstractions;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Jarvis.Models.Identity.Models.Identity;
using Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;
using System.Xml;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Database.Repositories.EntityFramework;
using Jarvis.Core.Multitenant;
using Microsoft.AspNetCore.Mvc.Razor;
using Jarvis.Core.Theming;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Services;
using Jarvis.Core.Permissions;
using Infrastructure.Abstractions.Validations;

namespace Jarvis.Core
{
    public static class ServiceCollectionExtension
    {
        public static void AddConfigJarvisDefault<T>(this IServiceCollection services) where T : DbContext
        {
            services.AddConfigJarvis();
            services.AddConfigMultitenant();
            services.AddConfigEntityFramework();
            services.AddConfigIdentity<T>();
            services.AddConfigSwagger();

            services.AddHttpContextAccessor();

            services.AddScoped<IValidatorFactory, ValidatorFactory>();
            services.AddScoped<IValidationContext, ValidationContext>();
        }

        public static void AddConfigJarvis(this IServiceCollection services)
        {
            //Base service
            services.AddSingleton<IModuleManager, ModuleManager>();
            services.AddSingleton<IPoliciesStorage, PoliciesStorage>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddScoped<IWorkContext, WorkContext>();

            //Bussiness Services
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IEntityService, EntityService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IOrganizationService, OrganizationService>();

            //Repositories
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IDistrictRepository, DistrictRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<ILabelRepository, LabelRepository>();
            services.AddScoped<IOrganizationUnitRepository, OrganizationUnitRepository>();
            services.AddScoped<IOrganizationUserRepository, OrganizationUserRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<ITokenInfoRepository, TokenInfoRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
        }

        public static void AddConfigMultitenant(this IServiceCollection services)
        {
            services.AddScoped<ITenantIdentificationService, HostTenantService>();
            services.AddScoped<ITenantIdentificationService, QueryTenantService>();
            services.AddScoped<ITenantService, TenantService>();
            //services.AddScoped<ICrudTenantService, CrudTenantService>();
        }

        public static void AddConfigAuthorization(this IServiceCollection services, List<Type> crudPolicies = null, List<Type> otherPolicies = null)
        {
            services.AddSingleton<IAuthorizationCrudPolicy, TenantAuthorizationCrudPolicy>();
            services.AddSingleton<IAuthorizationCrudPolicy, UserAuthorizationCrudPolicy>();
            services.AddSingleton<IAuthorizationCrudPolicy, RoleAuthorizationCrudPolicy>();
            services.AddSingleton<IAuthorizationCrudPolicy, LabelAuthorizationCrudPolicy>();
            services.AddSingleton<IAuthorizationCrudPolicy, SettingAuthorizationCrudPolicy>();
            services.AddSingleton<IAuthorizationCrudPolicy, OrganizationAuthorizationCrudPolicy>();

            services.AddSingleton<IAuthorizationPolicy, UserLockAuthorizationPolicy>();
            services.AddSingleton<IAuthorizationPolicy, UserResetPasswordAuthorizationPolicy>();

            if (crudPolicies != null)
            {
                foreach (var policy in crudPolicies)
                {
                    services.AddSingleton(typeof(IAuthorizationCrudPolicy), policy);
                }
            }

            if (otherPolicies != null)
            {
                foreach (var policy in otherPolicies)
                {
                    services.AddSingleton(typeof(IAuthorizationPolicy), policy);
                }
            }

            var serviceProvider = services.BuildServiceProvider();
            var crud = serviceProvider.GetServices<IAuthorizationCrudPolicy>();
            var other = serviceProvider.GetServices<IAuthorizationPolicy>();

            services.AddAuthorization(options =>
            {
                foreach (var policy in crud)
                {
                    options.AddPolicy($"{policy.Name}_Read", builder => builder.AddRequirements(new CrudRequirement($"{policy.Name}_Read")));
                    options.AddPolicy($"{policy.Name}_Create", builder => builder.AddRequirements(new CrudRequirement($"{policy.Name}_Create")));
                    options.AddPolicy($"{policy.Name}_Update", builder => builder.AddRequirements(new CrudRequirement($"{policy.Name}_Update")));
                    options.AddPolicy($"{policy.Name}_Delete", builder => builder.AddRequirements(new CrudRequirement($"{policy.Name}_Delete")));
                }

                foreach (var policy in other)
                {
                    options.AddPolicy(policy.Name, p => p.Requirements.Add(new CrudRequirement(policy.Name)));
                }


                // options.AddPolicy($"{policy.Type}_Read", policy.Build(httpContextAccessor.HttpContext, $"{policy.Type}_Read"));
                // options.AddPolicy($"{policy.Type}_Create", policy.Build(httpContextAccessor.HttpContext, $"{policy.Type}_Create"));
                // options.AddPolicy($"{policy.Type}_Update", policy.Build(httpContextAccessor.HttpContext, $"{policy.Type}_Update"));
                // options.AddPolicy($"{policy.Type}_Delete", policy.Build(httpContextAccessor.HttpContext, $"{policy.Type}_Delete"));

                // var moduleManager = serviceProvider.GetService<IModuleManager>();

                // var crudPolicies = moduleManager.GetInstances<IAuthorizationCrudPolicy>();
                // foreach (var policy in crudPolicies)
                // {
                //     //Các quyền CRUD ko sử dụng abstract
                //     options.AddPolicy($"{policy.Name}_Read", p => p.Requirements.Add(new CrudRequirement($"{policy.Name}_Read")));
                //     options.AddPolicy($"{policy.Name}_Create", p => p.Requirements.Add(new CrudRequirement($"{policy.Name}_Create")));
                //     options.AddPolicy($"{policy.Name}_Update", p => p.Requirements.Add(new CrudRequirement($"{policy.Name}_Update")));
                //     options.AddPolicy($"{policy.Name}_Delete", p => p.Requirements.Add(new CrudRequirement($"{policy.Name}_Delete")));

                //     //Các quyền CRUD sử dụng abstract
                //     options.AddPolicy($"*_Read", p => p.Requirements.Add(new CrudRequirement($"*_Read")));
                //     options.AddPolicy($"*_Create", p => p.Requirements.Add(new CrudRequirement($"*_Create")));
                //     options.AddPolicy($"*_Update", p => p.Requirements.Add(new CrudRequirement($"*_Update")));
                //     options.AddPolicy($"*_Delete", p => p.Requirements.Add(new CrudRequirement($"*_Delete")));
                // }
            });

            services.AddScoped<IAuthorizationHandler, AuthorizationCrudHandler>();
        }

        public static void AddConfigAuthentication(this IServiceCollection services, Action<AuthenticationOptions> authenticationOptions = null, Action<JwtBearerOptions> jwtOptions = null)
        {
            var provider = services.BuildServiceProvider();
            var configuration = provider.GetService<IConfiguration>();
            services.Configure<IdentityOption>(configuration.GetSection("Identity"));

            if (authenticationOptions == null)
            {
                authenticationOptions = options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                };
            }

            if (jwtOptions == null)
            {
                jwtOptions = options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateActor = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetSection("Identity:SecretKey").Value))
                    };
                };
            }

            services
                .AddAuthentication(authenticationOptions)
                .AddJwtBearer(jwtOptions);
        }

        public static void AddConfigIdentity<T>(this IServiceCollection services, Action<IdentityOptions> identityOptions = null) where T : DbContext
        {
            services
                .AddIdentity<User, Role>()
                .AddEntityFrameworkStores<T>()
                .AddDefaultTokenProviders();


            if (identityOptions == null)
            {
                identityOptions = options =>
                {
                    // Password settings
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 1;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;

                    // Lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                    options.Lockout.MaxFailedAccessAttempts = 3;
                    options.Lockout.AllowedForNewUsers = true;
                };
            }
            services.Configure<IdentityOptions>(identityOptions);

            //services.ConfigureApplicationCookie(options =>
            //{
            //    // Cookie settings
            //    options.Cookie.HttpOnly = true;
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

            //    options.LoginPath = "/Identity/Account/Login";
            //    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            //    options.SlidingExpiration = true;
            //});
        }

        public static void AddConfigValidator<T>(this IServiceCollection services, List<Type> rules) where T : class
        {
            foreach (var rule in rules)
            {
                services.AddScoped(typeof(IValidationRule), rule);
            }

            services.AddScoped<IValidator, Validator<T>>((provider) =>
            {
                return new Validator<T>(provider, rules);
            });
        }

        public static void AddTheming(this IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ThemeMultitenantViewLocationExpander());

                options.AreaViewLocationFormats.Clear();
                options.AreaViewLocationFormats.Add("/Areas/{2}/{1}/Views/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            });
        }

        public static void AddConfigSwagger(this IServiceCollection services, Dictionary<string, OpenApiInfo> versions = null)
        {
            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetService<IWebHostEnvironment>();
            var configuration = serviceProvider.GetService<IConfiguration>();

            services.AddSwaggerGen(options =>
            {
                //Các version API
                var name = Assembly.GetEntryAssembly().GetName().Name;
                if (versions == null)
                {
                    options.SwaggerDoc("v0", new OpenApiInfo { Title = $"{name} API", Version = "v0" });
                }
                else
                {
                    foreach (var version in versions)
                    {
                        options.SwaggerDoc(version.Key, version.Value);
                    }
                }

                //Comment mô tả API
                string rootPath;
                if (env.IsDevelopment())
                    rootPath = Path.Combine(env.ContentRootPath, "bin", "Debug", configuration.GetSection("ApplicationInfo:TargetFramework").Value);
                else
                    rootPath = env.ContentRootPath;

                var paths = Directory.GetFiles(rootPath, "*.xml");
                foreach (var path in paths)
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(path);

                    if (xmlDoc.SelectSingleNode("doc") == null)
                        continue;

                    options.IncludeXmlComments(path);
                }

                //Security
                options.CustomSchemaIds(x => x.FullName);
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Nhập JWT AccessToken ở API login",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                // options.OperationFilter<CustomHeaderOperationFilter>();
                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }

        private class CustomHeaderOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                // if (operation.Parameters == null)
                //     operation.Parameters = new List<IParameter>();

                // operation.Parameters.Add(new NonBodyParameter
                // {
                //     Name = "Envelope",
                //     In = "header",
                //     Type = "string",
                //     Required = false,
                //     Default = null,
                //     Description = "1 = Wrap, 0 = No wrap"
                // });

                // operation.Parameters.Add(new NonBodyParameter
                // {
                //     Name = "TenantCode",
                //     In = "query",
                //     Type = "string",
                //     Required = false,
                //     Default = null,
                //     Description = "Mã chi nhánh"
                // });
            }
        }

        private class SecurityRequirementsOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                // Policy names map to scopes
                var requiredScopes = context.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Select(attr => attr.Policy)
                    .Distinct()
                    .ToList();

                if (requiredScopes.Count == 0)
                    return;


                operation.Responses.Add("401", new OpenApiResponse() { Description = "Chưa đăng nhập" });
                operation.Responses.Add("403", new OpenApiResponse() { Description = "Không có quyền" });

                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                };

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [ oAuthScheme ] = requiredScopes.ToList()
                    }
                };
            }

        }
    }
}
