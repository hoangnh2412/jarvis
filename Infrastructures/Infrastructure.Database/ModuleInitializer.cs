//using Infrastructure.Database.Abstractions;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;
//using System.Linq;
//using System.Reflection;

//namespace Infrastructure.Database
//{
//    public class ModuleInitializer : InfrastructureInitializer
//    {
//        public override void Configure(IApplicationBuilder app)
//        {

//        }

//        public override void ConfigureServices(IServiceCollection services)
//        {
//            var serviceProvider = services.BuildServiceProvider();
//            var moduleManager = serviceProvider.GetService<IModuleManager>();

//            var interfaceRepos = moduleManager.GetInterfaces<IRepository>().ToList();
//            var implementRepos = moduleManager.GetImplementations<IRepository>().Where(x => !x.GetTypeInfo().IsAbstract).ToList();

//            foreach (var interfaceRepo in interfaceRepos)
//            {
//                var repos = implementRepos.Where(x => x.GetInterfaces().Any(y => y.FullName == interfaceRepo.FullName)).ToList();
//                foreach (var repo in repos)
//                {
//                    services.AddScoped(interfaceRepo, repo);
//                }
//            }
//        }
//    }
//}