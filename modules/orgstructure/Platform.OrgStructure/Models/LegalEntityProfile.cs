namespace Platform.OrgStructure.Models;

/// <summary>Phần hồ sơ pháp nhân ngoài mã / tên / MST (cùng shape form thông tin doanh nghiệp của host).</summary>
public sealed class LegalEntityProfilePayload
{
    public string Address { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string BankAccount { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    public string PaymentCode { get; set; } = string.Empty;

    public string Delegate { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string LegalRepPhone { get; set; } = string.Empty;

    public string LegalRepDob { get; set; } = string.Empty;

    public string LegalRepPassport { get; set; } = string.Empty;

    public string IdentityCard { get; set; } = string.Empty;

    public bool UseIdentityCard { get; set; }

    public int LegalRepGender { get; set; } = 1;

    public string Email { get; set; } = string.Empty;

    public string Tel { get; set; } = string.Empty;

    public string WebSite { get; set; } = string.Empty;

    public string Fax { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;

    public string TaxAuthorityName { get; set; } = string.Empty;

    public string AgentInfo { get; set; } = string.Empty;
}

public sealed class LegalEntityApiModel
{
    public Guid Id { get; init; }

    public Guid OrgId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? TaxId { get; init; }

    public bool IsActive { get; init; }

    public LegalEntityProfilePayload Profile { get; init; } = new();
}
