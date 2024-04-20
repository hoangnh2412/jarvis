namespace Jarvis.Application.MultiTenancy;

public interface ITenantConnectionStringResolver
{
    string GetConnectionString(string tenantIdOrName = null);
}