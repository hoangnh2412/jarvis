using System;

namespace Jarvis.Core.Models.Organizations
{
    public class GetOrganizationUserResponseModel
    {
        public int Id { get; set; }
        public Guid UserCode { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public Guid OrganizationCode { get; set; }
        public int Level { get; set; }
    }
}