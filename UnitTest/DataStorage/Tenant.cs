using System.ComponentModel.DataAnnotations.Schema;
using Jarvis.Domain.Entities;

namespace UnitTest.DataStorage;

public class Tenant : ITenant
{
    [Column("name")]
    public string Name { get; set; }

    [Column("connectionstring")]
    public string ConnectionString { get; set; }

    [Column("id")]
    public Guid Id { get; set; }
}