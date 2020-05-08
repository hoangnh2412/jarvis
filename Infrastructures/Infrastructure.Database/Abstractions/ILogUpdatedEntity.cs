using System;

namespace Infrastructure.Database.Abstractions
{
    public interface ILogUpdatedEntity
    {
        DateTime? UpdatedAt { get; set; }
        DateTime? UpdatedAtUtc { get; set; }
        Guid? UpdatedBy { get; set; }
    }
}
