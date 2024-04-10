using Jarvis.Domain.Entities;

namespace UnitTest.DataStorage;

public class Tenant : ITenant
{
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public Guid Id { get; set; }
}