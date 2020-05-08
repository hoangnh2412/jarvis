using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Database.Abstractions
{
    public interface ITenantEntity
    {
        Guid TenantCode { get; set; }
    }
}
