using Jarvis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sample.Entities;

public class Student : BaseEntity<Guid>, ITenantEntity
{
    public required string Name { get; set; }
    public int Age { get; set; }
    public Guid TenantId { get; set; }
}

public class StudentEntityConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Student");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
    }
}