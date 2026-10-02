namespace Platform.Authorization.EntityFramework.Persistence.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Authentication.Identity.Domain;

/// <summary>
/// Cấu hình EF cho thực thể Role.
/// Mở rộng bảng AspNetRoles bằng RoleLevel, IsSystemRole, DisplayName.
/// </summary>
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Sử dụng bảng AspNetRoles mặc định; chỉ cấu hình các thuộc tính bổ sung
        builder.ToTable("AspNetRoles");

        builder.Property(r => r.RoleLevel)
            .HasDefaultValue(100);

        builder.Property(r => r.IsSystemRole)
            .HasDefaultValue(false);

        builder.Property(r => r.DisplayName)
            .HasMaxLength(255);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Chỉ mục cho RoleLevel (được sử dụng trong kiểm tra quản trị)
        builder.HasIndex(r => r.RoleLevel);
    }
}
