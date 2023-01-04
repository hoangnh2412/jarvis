using System;
using System.Collections.Generic;

namespace Jarvis.Core.Models.Organizations
{
    public class CreateOrganizationUserRequestModel
    {
        public Guid UnitCode { get; set; }
        public List<Guid> UserCodes { get; set; }
    }
}