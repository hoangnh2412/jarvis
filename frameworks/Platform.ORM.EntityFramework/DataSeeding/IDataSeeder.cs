namespace Platform.ORM.EntityFramework.DataSeeding;

/// <summary>
/// Một bước seed qua EF sau migrate. Host đăng ký nhiều implementation;
/// <see cref="DataSeed.RunAllAsync"/> gọi theo <see cref="Order"/>.
/// </summary>
public interface IDataSeeder
{
    /// <summary>Thứ tự tăng dần (catalog/metadata trước, dữ liệu phụ thuộc sau).</summary>
    int Order { get; }

    Task SeedAsync(CancellationToken cancellationToken = default);
}
