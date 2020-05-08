using System;

namespace Infrastructure.Database.Abstractions
{
    public interface ILogDeletedEntity
    {
        DateTime? DeletedAt { get; set; }
        DateTime? DeletedAtUtc { get; set; }
        Guid? DeletedBy { get; set; }
    }
}
