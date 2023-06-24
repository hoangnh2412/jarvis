using System.Collections.Generic;
using Jarvis.Core.Abstractions;

namespace Jarvis.Core.Permissions
{
    public abstract class CrudClaimAction : IClaimAction
    {
        public List<ClaimAction> Actions = new List<ClaimAction>();

        public abstract string Name { get; }

        public List<ClaimAction> GetActions()
        {
            Actions.AddRange(GetCrudActions());
            return Actions;
        }

        public List<ClaimAction> GetCrudActions()
        {
            return new List<ClaimAction> {
                new ClaimAction { Id = 1, Name = "Read", Description = "Xem" },
                new ClaimAction { Id = 2, Name = "Create", Description = "Tạo" },
                new ClaimAction { Id = 4, Name = "Update", Description = "Sửa" },
                new ClaimAction { Id = 8, Name = "Delete", Description = "Xoá" },
                new ClaimAction { Id = 16, Name = "Import", Description = "Import" },
                new ClaimAction { Id = 32, Name = "Export", Description = "Export" },
            };
        }
    }
}