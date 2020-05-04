using System;

namespace Infrastructure.Database.Abstractions
{
    public interface ILogCreatedEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime CreatedAtUtc { get; set; }
        Guid CreatedBy { get; set; }
    }
}
