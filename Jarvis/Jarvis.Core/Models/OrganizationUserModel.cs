using System;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Models
{
    public class OrganizationUserModel
    {
        public Guid IdUser { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public Guid OrganizationCode { get; set; }

        public static implicit operator OrganizationUserModel(OrganizationUser entity)
        {
            if (entity == null)
                return null;

            return new OrganizationUserModel
            {
                IdUser = entity.IdUser,
                OrganizationCode = entity.OrganizationCode
            };
        }
    }
}