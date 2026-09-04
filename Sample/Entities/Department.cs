using Jarvis.DDD.Domain.Entities;
using Newtonsoft.Json;

namespace Sample.Entities;

public class Department : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    
    [JsonIgnore]
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
