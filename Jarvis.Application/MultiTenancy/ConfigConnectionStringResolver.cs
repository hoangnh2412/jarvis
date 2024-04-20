using Microsoft.Extensions.Configuration;

namespace Jarvis.Application.MultiTenancy;

public class ConfigConnectionStringResolver : ITenantConnectionStringResolver
{
    private readonly IConfiguration _configuration;

    public ConfigConnectionStringResolver(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetConnectionString(string tenantIdOrName = null)
    {
        return _configuration.GetConnectionString(tenantIdOrName);
    }
}