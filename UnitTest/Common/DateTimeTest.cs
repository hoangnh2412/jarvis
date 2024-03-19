// using Microsoft.Extensions.DependencyInjection;
// using Jarvis.Infrastructure.Extensions;

// namespace UnitTest.Common;

// [TestClass]
// public class DateTimeTest : BaseTest
// {
//     private IServiceProvider _serviceProvider;

//     [TestInitialize]
//     public void TestInitialize()
//     {
//         var services = new ServiceCollection();
//         _serviceProvider = services.BuildServiceProvider();
//     }

//     [TestMethod]
//     public void Test_DateTime_To_UnixTime_Second()
//     {
//         var datetime = new DateTime(2023, 12, 7, 4, 32, 14, DateTimeKind.Utc);
//         var timestamp = DateTimeExtension.ToUnixTimeSecond(datetime);
//         Assert.AreEqual(1701923534, timestamp);
//     }

//     [TestMethod]
//     public void Test_DateTime_To_UnixTime_Millisecond()
//     {
//         var datetime = new DateTime(2023, 12, 7, 4, 32, 14, DateTimeKind.Utc);
//         var timestamp = DateTimeExtension.ToUnixTimeMillisecond(datetime);
//         Assert.AreEqual(1701923534000, timestamp);
//     }

//     [TestMethod]
//     public void Test_UnixTime_Second_To_DateTime()
//     {
//         var datetime = NumberExtension.FromUnixTimeSecond(1701923534);
//         Assert.AreEqual(new DateTime(2023, 12, 7, 4, 32, 14, DateTimeKind.Utc), datetime);
//     }

//     [TestMethod]
//     public void Test_UnixTime_Millisecond_To_DateTime()
//     {
//         var datetime = NumberExtension.FromUnixTimeMillisecond(1701923534000);
//         Assert.AreEqual(new DateTime(2023, 12, 7, 4, 32, 14, DateTimeKind.Utc), datetime);
//     }
// }