using Jarvis.Core.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Models
{
    public class SessionModel
    {
        public Guid IdUser { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, KeyValuePair<ClaimOfResource, ClaimOfChildResource>> Claims { get; set; }
        public SessionInfoModel UserInfo { get; set; }
        public SessionTenantModel TenantInfo { get; set; }
        public SessionOrganizationModel OrganizationInfo { get; set; }
    }

    public class SessionInfoModel
    {
        public string FullName { get; set; }
        public string AvatarPath { get; set; }
    }

    public class SessionTenantModel
    {
        public Guid Code { get; set; }
        public string Theme { get; set; }
        public string TaxCode { get; set; }
        public string FullNameVi { get; set; }
        public string FullNameEn { get; set; }
    }

    public class SessionOrganizationModel
    {
        public Guid Code { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
    }
}
