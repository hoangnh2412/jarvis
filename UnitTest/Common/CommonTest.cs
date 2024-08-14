using Microsoft.Extensions.DependencyInjection;
using Jarvis.Domain.Attributes;
using Jarvis.Domain.Common;
using Jarvis.Shared.Extensions;

namespace UnitTest.Common;

[TestClass]
public class CommonTest : BaseTest
{
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
        var services = new ServiceCollection();
        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public void Test_Compare_Type()
    {
        // var result = typeof(SampleDbContext).GetInterfaces().Contains(typeof(IStorageContext));
        // Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void Test_Reflection_GetPropertyByAttributeName()
    {
        var field = TypeExtension.GetPropertyByAttribute(typeof(BaseEntity), typeof(CodeAttribute));
        Assert.IsNull(field);
    }
}