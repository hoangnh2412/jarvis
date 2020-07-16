using System;

namespace Jarvis.Core.Models.Events.Tenants
{
    public class TenantDeletedEventModel
    {
        public Guid TenantCode { get; set; }
    }
}