using Infrastructure.Database.Entities;
using System;
using System.Collections.Generic;

namespace Jarvis.Models.Identity.Models.Identity
{
    public class RoleModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Claims { get; set; }

        public static implicit operator RoleModel(Role entity)
        {
            if (entity == null)
                return null;

            var model = new RoleModel();
            model.Id = entity.Id;
            model.Name = entity.Name;
            return model;
        }
    }
}
