using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Platform.Modules.CompanyProfile.EntityFramework;

/// <summary>Bảng lõi hồ sơ doanh nghiệp, khóa theo (OwnerType, OwnerId). Host map vào schema của mình.</summary>
public sealed class CompanyProfileEntity
{
    public CompanyProfileOwnerType OwnerType { get; set; }

    public Guid OwnerId { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public string? LogoUrl { get; set; }

    public string? Representative { get; set; }

    public string? RepresentativeTitle { get; set; }

    public string? BankAccount { get; set; }

    public string? BankName { get; set; }

    public string? Province { get; set; }

    public string? PaymentCode { get; set; }

    public string? LegalRepPhone { get; set; }

    public DateOnly? LegalRepDob { get; set; }

    public string? LegalRepPassport { get; set; }

    public string? IdentityCard { get; set; }

    public bool UseIdentityCard { get; set; }

    public int? LegalRepGender { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

/// <summary>Cửa truy cập DbContext chứa bảng company profile của host.</summary>
public interface ICompanyProfileDbContextProvider
{
    DbContext DbContext { get; }
}

public static class CompanyProfileModelBuilderExtensions
{
    public static ModelBuilder ApplyCompanyProfileModel(this ModelBuilder modelBuilder, string schema, string table = "company_profile")
    {
        modelBuilder.Entity<CompanyProfileEntity>(entity =>
        {
            entity.ToTable(table, schema);
            entity.HasKey(profile => new { profile.OwnerType, profile.OwnerId });
            entity.Property(profile => profile.Address).HasMaxLength(1000);
            entity.Property(profile => profile.Phone).HasMaxLength(30);
            entity.Property(profile => profile.Fax).HasMaxLength(30);
            entity.Property(profile => profile.Email).HasMaxLength(320);
            entity.Property(profile => profile.Website).HasMaxLength(1000);
            entity.Property(profile => profile.LogoUrl).HasMaxLength(1000);
            entity.Property(profile => profile.Representative).HasMaxLength(400);
            entity.Property(profile => profile.RepresentativeTitle).HasMaxLength(400);
            entity.Property(profile => profile.BankAccount).HasMaxLength(50);
            entity.Property(profile => profile.BankName).HasMaxLength(400);
            entity.Property(profile => profile.Province).HasMaxLength(400);
            entity.Property(profile => profile.PaymentCode).HasMaxLength(50);
            entity.Property(profile => profile.LegalRepPhone).HasMaxLength(30);
            entity.Property(profile => profile.LegalRepPassport).HasMaxLength(50);
            entity.Property(profile => profile.IdentityCard).HasMaxLength(50);
        });
        return modelBuilder;
    }
}
