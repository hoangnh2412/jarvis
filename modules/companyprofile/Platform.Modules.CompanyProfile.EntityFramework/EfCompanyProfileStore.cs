using Microsoft.EntityFrameworkCore;

namespace Platform.Modules.CompanyProfile.EntityFramework;

public sealed class EfCompanyProfileStore(ICompanyProfileDbContextProvider dbProvider) : ICompanyProfileStore
{
    public async Task<CompanyProfileContent?> GetAsync(CompanyProfileOwner owner, CancellationToken cancellationToken = default)
    {
        var entity = await dbProvider.DbContext.Set<CompanyProfileEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.OwnerType == owner.Type && row.OwnerId == owner.Id, cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : Map(entity);
    }

    public async Task SaveAsync(CompanyProfileOwner owner, CompanyProfileContent content, CancellationToken cancellationToken = default)
    {
        var db = dbProvider.DbContext;
        var entity = await db.Set<CompanyProfileEntity>()
            .FirstOrDefaultAsync(row => row.OwnerType == owner.Type && row.OwnerId == owner.Id, cancellationToken)
            .ConfigureAwait(false);
        var now = DateTime.UtcNow;
        if (entity is null)
        {
            entity = new CompanyProfileEntity { OwnerType = owner.Type, OwnerId = owner.Id, CreatedAt = now };
            db.Set<CompanyProfileEntity>().Add(entity);
        }

        Apply(entity, content);
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static CompanyProfileContent Map(CompanyProfileEntity entity) => new()
    {
        Address = entity.Address,
        Phone = entity.Phone,
        Fax = entity.Fax,
        Email = entity.Email,
        Website = entity.Website,
        LogoUrl = entity.LogoUrl,
        Representative = entity.Representative,
        RepresentativeTitle = entity.RepresentativeTitle,
        BankAccount = entity.BankAccount,
        BankName = entity.BankName,
        Province = entity.Province,
        PaymentCode = entity.PaymentCode,
        LegalRepPhone = entity.LegalRepPhone,
        LegalRepDob = entity.LegalRepDob,
        LegalRepPassport = entity.LegalRepPassport,
        IdentityCard = entity.IdentityCard,
        UseIdentityCard = entity.UseIdentityCard,
        LegalRepGender = entity.LegalRepGender,
    };

    private static void Apply(CompanyProfileEntity entity, CompanyProfileContent content)
    {
        entity.Address = content.Address;
        entity.Phone = content.Phone;
        entity.Fax = content.Fax;
        entity.Email = content.Email;
        entity.Website = content.Website;
        entity.LogoUrl = content.LogoUrl;
        entity.Representative = content.Representative;
        entity.RepresentativeTitle = content.RepresentativeTitle;
        entity.BankAccount = content.BankAccount;
        entity.BankName = content.BankName;
        entity.Province = content.Province;
        entity.PaymentCode = content.PaymentCode;
        entity.LegalRepPhone = content.LegalRepPhone;
        entity.LegalRepDob = content.LegalRepDob;
        entity.LegalRepPassport = content.LegalRepPassport;
        entity.IdentityCard = content.IdentityCard;
        entity.UseIdentityCard = content.UseIdentityCard;
        entity.LegalRepGender = content.LegalRepGender;
    }
}
