using System;

namespace Jarvis.Core.Models.Events.Tenants
{
    public class TenantCreatedEventModel
    {
        public Guid TenantCode { get; set; }
        public Guid IdUserRoot { get; set; }
        public string PasswordUserRoot { get; set; }
        public Guid IdUserAdmin { get; set; }
        public string PasswordUserAdmin { get; set; }
        public bool IsRandomPasswordAdmin { get; set; }
    }
}