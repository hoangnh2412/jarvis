using Jarvis.DDD.Domain.Entities;

namespace Sample.Entities;

public class Employee : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = default!;
    public decimal BaseSalary { get; set; }
    public bool IsActive { get; set; }

    public string IdentityNumber { get; set; } = default!; // Số CCCD / CMND
    public string BankAccountNumber { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;
    
    public Guid PositionId { get; set; }
    public Position Position { get; set; } = default!;
}
