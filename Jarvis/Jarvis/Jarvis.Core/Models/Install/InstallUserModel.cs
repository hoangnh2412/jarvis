using System;
using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Models.Install
{
    public class InstallUserModel
    {
        public Guid TenantCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }
}
