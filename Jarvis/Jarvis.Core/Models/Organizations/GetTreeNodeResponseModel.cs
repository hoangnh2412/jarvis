using System;
using System.Collections.Generic;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Models.Organizations
{
    public class GetTreeNodeResponseModel
    {
        public Guid Code { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public Guid? IdParent { get; set; }
        public Guid TenantCode { get; set; }
        public bool Collapsed { get; set; }
        public List<GetTreeNodeResponseModel> Nodes { get; set; }

        public static implicit operator GetTreeNodeResponseModel(OrganizationUnit entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new GetTreeNodeResponseModel
            {
                Code = entity.Code,
                Name = entity.Name,
                FullName = entity.FullName,
                Description = entity.Description,
                IdParent = entity.IdParent,
                TenantCode = entity.TenantCode,
                Collapsed = false
            };
        }
    }
}