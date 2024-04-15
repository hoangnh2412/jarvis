using Jarvis.Domain.Common.Interfaces;

namespace Sample.DataStorage;

public class User : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}