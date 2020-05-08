namespace Jarvis.Core.Models.Tenant
{
    public class UpdateTenantCommand
    {
        public string HostName { get; set; }
        public TenantInfoModel Info { get; set; }
    }
}