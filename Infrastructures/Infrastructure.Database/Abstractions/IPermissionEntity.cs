using System;

namespace Infrastructure.Database.Abstractions
{
    public interface IPermissionEntity : ITenantEntity, ILogCreatedEntity
    {
        new Guid TenantCode { get; set; }
        new Guid CreatedBy { get; set; }
    }
}
