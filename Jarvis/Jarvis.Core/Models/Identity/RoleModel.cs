using Infrastructure.Database.Entities;
using System;
using System.Collections.Generic;

namespace Jarvis.Models.Identity.Models.Identity
{
    public class RoleModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string[] FunctionClaims { get; set; }
        public string[] DataClaims { get; set; }

        public static implicit operator RoleModel(Role entity)
        {
            if (entity == null)
                return null;

            var model = new RoleModel();
            model.Id = entity.Key;
            model.Name = entity.Name;
            return model;
        }
    }
}
