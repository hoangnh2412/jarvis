using Microsoft.Extensions.Configuration;

namespace Jarvis.Application.MultiTenancy;

public class SingleTenantConnectionStringResolver : IConnectionStringResolver
{
    private readonly IConfiguration _configuration;

    public SingleTenantConnectionStringResolver(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> GetConnectionStringAsync(string name)
    {
        return Task.FromResult(_configuration.GetConnectionString(name));
    }
}