using Microsoft.Extensions.Configuration;

namespace UnitTest
{
    public class BaseTest
    {
        public Microsoft.Extensions.Configuration.IConfiguration Configuration;

        public BaseTest()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}