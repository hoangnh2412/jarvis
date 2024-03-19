// using Microsoft.Extensions.DependencyInjection;
// using Jarvis.Infrastructure.Database.Repositories;
// using Jarvis.Infrastructure.Database.EntityFramework;
// using Microsoft.EntityFrameworkCore;
// using Sample.Database.MySql;
// using Microsoft.Extensions.Configuration;
// using Sample.Database;
// using Sample.Database.Poco;

// namespace UnitTest.DataStorage;

// [TestClass]
// public class DataAccessMySqlTest : BaseTest
// {
//     private IServiceProvider _serviceProvider;

//     [TestInitialize]
//     public void TestInitialize()
//     {
//         var services = new ServiceCollection();
//         //services.AddSampleDbContext(Configuration.GetConnectionString("MySql"));
//         services.AddConfigEntityFramework();
//         _serviceProvider = services.BuildServiceProvider();
//     }

//     [TestMethod]
//     public async Task Test_DataAccess_GenericRepository_CRUD()
//     {
//         var uow = _serviceProvider.GetService<ISampleUnitOfWork>();
//         var repo = uow.GetRepository<IRepository<Student>>();

//         // List
//         var items = await repo.ListAsync();
//         Assert.AreNotEqual(0, items.Count);

//         // Insert
//         var key = Guid.NewGuid();
//         var random = new Random();
//         var entity = await repo.InsertAsync(new Student
//         {
//             Key = key,
//             Code = $"P_{key.ToString("N")}",
//             Name = $"Patient {key.ToString("N")}",
//             Age = random.Next(20, 50)
//         });
//         await uow.CommitAsync();
//         Assert.AreEqual(items.Count + 1, await repo.CountAsync());

//         // FindByKey
//         var item = await repo.GetQuery().FirstOrDefaultAsync(x => x.Key == key);
//         Assert.IsNotNull(item);

//         // Update
//         var newKey = Guid.NewGuid();
//         item.Key = newKey;
//         item.Code = $"P_{newKey.ToString("N")}";
//         item.Name = $"Patient {newKey.ToString("N")}";
//         item.Age = random.Next(20, 50);
//         await repo.UpdateAsync(item);
//         await uow.CommitAsync();
//         item = await repo.GetQuery().FirstOrDefaultAsync(x => x.Key == newKey);
//         Assert.AreEqual($"P_{newKey.ToString("N")}", item.Code);

//         // Delete
//         await repo.DeleteAsync(item);
//         await uow.CommitAsync();
//         Assert.AreEqual(items.Count, await repo.CountAsync());
//     }
// }