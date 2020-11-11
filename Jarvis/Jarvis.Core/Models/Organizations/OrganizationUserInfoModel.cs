using System;

namespace Jarvis.Core.Models.Organizations
{
    public class OrganizationUserInfoModel
    {
        public Guid Code { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
    }
}