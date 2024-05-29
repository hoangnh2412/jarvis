namespace Sample.Application.DTOs;

public class UserTenantDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int Age { get; set; }

    public Guid TenantId { get; set; }

    public string ConnectionString { get; set; }
}