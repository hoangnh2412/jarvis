using Jarvis.DDD.Domain.Entities;

namespace UnitTest.Querying;

public class TestEntity : IEntity, ILogCreatedEntity, ILogUpdatedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string FirstName { get; set; } = default!;
    
    public string LastName { get; set; } = default!;
    
    public int Age { get; set; }
    
    public decimal Amount { get; set; }
    
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public Guid UpdatedBy { get; set; }
    
    public string? NullableField { get; set; }
    
    public DateOnly Birthday { get; set; }
    
    public TestStatus Status { get; set; }
    
    public double DoubleValue { get; set; }
    
    public float FloatValue { get; set; }
    
    public DateTimeOffset DateTimeOffsetValue { get; set; }
}

public enum TestStatus
{
    Pending = 0,
    Active = 1,
    Closed = 2
}

public class NoAuditEntity : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
}

public class OnlyCreatedEntity : IEntity, ILogCreatedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}

// Giả lập một entity siêu dị không có cột Id (mặc dù thực tế IEntity bắt buộc có Id, 
// nhưng để test logic Reflection của DefaultSort khi Id không tồn tại, ta sẽ bypass IEntity bằng dynamic IQueryable hoặc tạo class không kế thừa IEntity để test riêng QueryableDefaultSortExtensions)
public class NoIdEntity : IEntity
{
    public string Name { get; set; } = default!;
}
