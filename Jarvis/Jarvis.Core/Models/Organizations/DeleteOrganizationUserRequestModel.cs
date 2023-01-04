using System;

namespace Jarvis.Core.Models.Organizations
{
    public class DeleteOrganizationUserRequestModel
    {
        public Guid OrganizationUnitCode { get; set; }
        public Guid OrganizationUserCode { get; set; }
    }
}