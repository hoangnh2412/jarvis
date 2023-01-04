using System;

namespace Jarvis.Core.Models.Organizations
{
    public class GetOrganizationUnitResponseModel
    {
        public Guid Code { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public Guid? IdParent { get; set; }

        public Guid TenantCode { get; set; }
    }
}