using Jarvis.DDD.Domain.Entities;
using Newtonsoft.Json;

namespace Sample.Entities;

public class Position : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = default!;
    public int Level { get; set; }
    
    [JsonIgnore]
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
