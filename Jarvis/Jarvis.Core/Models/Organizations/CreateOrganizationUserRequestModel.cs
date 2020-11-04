using System;

namespace Jarvis.Core.Models.Organizations
{
    public class CreateOrganizationUserRequestModel
    {
        public Guid OrganizationUnitCode { get; set; }
        public Guid OrganizationUserCode { get; set; }
        public int Level { get; set; }
    }
}