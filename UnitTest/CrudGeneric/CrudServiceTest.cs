// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Jarvis.Infrastructure.Database.EntityFramework;
// using Sample.Database.MySql;
// using Jarvis.Services;
// using Sample.Application.Catalog.Students.Services;

// namespace UnitTest.CrudGeneric
// {
//     [TestClass]
//     public class CrudServiceTest : BaseTest
//     {
//         private IServiceProvider _serviceProvider;

//         [TestInitialize]
//         public void TestInitialize()
//         {
//             var services = new ServiceCollection();
//             //services.AddSampleDbContext(Configuration.GetConnectionString("MySql"));
//             services.AddConfigEntityFramework();

//             services.AddSingleton<IWorkContext, WorkContextTest>();
//             services.AddSingleton<IStudentCrudService, StudentCrudService>();

//             _serviceProvider = services.BuildServiceProvider();
//         }

//         [TestMethod]
//         public async Task Test_CrudGeneric_Basic_Create()
//         {
//             var service = _serviceProvider.GetService<IStudentCrudService>();
//             var result = await service.CreateAsync(new Sample.Application.Catalog.Students.Models.StudentCreateInput
//             {
//                 UserId = Guid.NewGuid(),
//                 Code = "hoangnh",
//                 Name = "HoangNH",
//                 Age = 32
//             });

//             Assert.AreEqual(result, 1);
//         }

//         [TestMethod]
//         public async Task Test_CrudGeneric_Basic_Update()
//         {
//             var service = _serviceProvider.GetService<IStudentCrudService>();
//             var result = await service.UpdateAsync(Guid.Parse("38dcc8e7-9b14-4468-8ab0-83676a303074"), new Sample.Application.Catalog.Students.Models.StudentUpdateInput
//             {
//                 Name = "HoangNH91",
//                 Age = 33
//             });

//             Assert.AreEqual(result, 1);
//         }

//         [TestMethod]
//         public async Task Test_CrudGeneric_Basic_Delete()
//         {
//             var service = _serviceProvider.GetService<IStudentCrudService>();
//             var result = await service.DeleteAsync(Guid.Parse("38dcc8e7-9b14-4468-8ab0-83676a303074"));
//             Assert.AreEqual(result, 1);
//         }
//     }
// }