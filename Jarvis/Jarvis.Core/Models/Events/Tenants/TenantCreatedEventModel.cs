using System;

namespace Jarvis.Core.Models.Events.Tenants
{
    public class TenantCreatedEventModel
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string Password { get; set; }
    }
}