using Infrastructure.Database.Entities;
using System;
using System.Collections.Generic;

namespace Jarvis.Models.Identity.Models.Identity
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

        public UserInfoModel Infos { get; set; }
        public string Metadata { get; set; }
        public List<Guid> IdRoles { get; set; }
        public List<string> Claims { get; set; }

        public static implicit operator UserModel(User entity)
        {
            if (entity == null)
                return null;

            var model = new UserModel();
            model.Id = entity.Id;
            model.UserName = entity.UserName;
            model.Email = entity.Email;
            model.PhoneNumber = entity.PhoneNumber;
            model.LockoutEnd = entity.LockoutEnd;
            model.CreatedAt = entity.CreatedAt;
            model.LockoutEnd = entity.LockoutEnd;
            return model;
        }
    }
}
