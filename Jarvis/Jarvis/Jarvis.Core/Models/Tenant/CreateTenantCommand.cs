namespace Jarvis.Core.Models.Tenant
{
    public class CreateTenantCommand
    {
        public string HostName { get; set; }
        public TenantInfoModel Info { get; set; }
        public TenantUserModel User { get; set; }
    }
}