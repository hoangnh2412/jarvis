using System.Text.Json;
using System.Text.Json.Nodes;

namespace Platform.Modules.CompanyProfile;

/// <summary>Chủ sở hữu hồ sơ doanh nghiệp: tenant (thông tin doanh nghiệp) hoặc pháp nhân trong cơ cấu tổ chức.</summary>
public enum CompanyProfileOwnerType
{
    Tenant = 0,
    LegalEntity = 1,
}

public sealed record CompanyProfileOwner(CompanyProfileOwnerType Type, Guid Id);

/// <summary>Phần lõi hồ sơ doanh nghiệp dùng chung mọi sản phẩm (người bán trên hóa đơn, liên hệ, ngân hàng, đại diện).</summary>
public sealed class CompanyProfileContent
{
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
}

/// <summary>Hồ sơ đầy đủ trả cho API/UI: lõi + các khối mở rộng theo <see cref="ICompanyProfileExtension.Key"/>.</summary>
public sealed record CompanyProfileDto(
    CompanyProfileOwner Owner,
    CompanyProfileContent Core,
    IReadOnlyDictionary<string, JsonObject> Extensions);

/// <summary>
/// Điểm mở rộng hồ sơ theo sản phẩm: mỗi extension sở hữu bảng/kho riêng, đọc-ghi theo lô.
/// Payload là JSON object; extension tự validate và map sang kho của mình.
/// </summary>
public interface ICompanyProfileExtension
{
    string Key { get; }

    Task<IReadOnlyDictionary<CompanyProfileOwner, JsonObject>> LoadAsync(
        IReadOnlyCollection<CompanyProfileOwner> owners,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        CompanyProfileOwner owner,
        JsonElement payload,
        CancellationToken cancellationToken = default);
}

/// <summary>Kho phần lõi. Host cắm EF qua module EntityFramework hoặc tự implement.</summary>
public interface ICompanyProfileStore
{
    Task<CompanyProfileContent?> GetAsync(CompanyProfileOwner owner, CancellationToken cancellationToken = default);

    Task SaveAsync(CompanyProfileOwner owner, CompanyProfileContent content, CancellationToken cancellationToken = default);
}

/// <summary>Đọc hồ sơ cho code nghiệp vụ (người bán trên hóa đơn…) — luôn kèm extension đã đăng ký.</summary>
public interface ICompanyProfileReader
{
    Task<CompanyProfileDto> GetAsync(CompanyProfileOwner owner, CancellationToken cancellationToken = default);
}

public interface ICompanyProfileService : ICompanyProfileReader
{
    Task<CompanyProfileDto> SaveAsync(
        CompanyProfileOwner owner,
        CompanyProfileContent core,
        IReadOnlyDictionary<string, JsonElement>? extensions = null,
        CancellationToken cancellationToken = default);
}

public sealed class CompanyProfileService(
    ICompanyProfileStore store,
    IEnumerable<ICompanyProfileExtension> extensions) : ICompanyProfileService
{
    private readonly IReadOnlyList<ICompanyProfileExtension> _extensions = extensions.ToList();

    public async Task<CompanyProfileDto> GetAsync(CompanyProfileOwner owner, CancellationToken cancellationToken = default)
    {
        var core = await store.GetAsync(owner, cancellationToken).ConfigureAwait(false) ?? new CompanyProfileContent();
        var map = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        foreach (var extension in _extensions)
        {
            var loaded = await extension.LoadAsync([owner], cancellationToken).ConfigureAwait(false);
            if (loaded.TryGetValue(owner, out var payload))
                map[extension.Key] = payload;
        }

        return new CompanyProfileDto(owner, core, map);
    }

    public async Task<CompanyProfileDto> SaveAsync(
        CompanyProfileOwner owner,
        CompanyProfileContent core,
        IReadOnlyDictionary<string, JsonElement>? extensions = null,
        CancellationToken cancellationToken = default)
    {
        await store.SaveAsync(owner, core, cancellationToken).ConfigureAwait(false);
        if (extensions is not null)
        {
            foreach (var (key, payload) in extensions)
            {
                var extension = _extensions.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.Ordinal))
                    ?? throw new ArgumentException($"Chưa đăng ký company profile extension '{key}'.", nameof(extensions));
                await extension.SaveAsync(owner, payload, cancellationToken).ConfigureAwait(false);
            }
        }

        return await GetAsync(owner, cancellationToken).ConfigureAwait(false);
    }
}
