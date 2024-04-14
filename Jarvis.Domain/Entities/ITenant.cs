using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Domain.Entities;

public interface ITenant : IEntity<Guid>
{
    public string Name { get; set; }
    public string ConnectionString { get; set; }
}