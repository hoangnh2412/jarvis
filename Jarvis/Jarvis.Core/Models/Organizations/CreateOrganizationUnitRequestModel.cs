using System;

namespace Jarvis.Core.Models.Organizations
{
    public class CreateOrganizationUnitRequestModel
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public Guid IdParent { get; set; }
    }
}