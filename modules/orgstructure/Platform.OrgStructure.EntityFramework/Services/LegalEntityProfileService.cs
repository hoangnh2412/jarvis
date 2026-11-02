using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Platform.OrgStructure.EntityFramework.Persistence;
using Platform.OrgStructure.Models;
using Platform.OrgStructure.Services;

namespace Platform.OrgStructure.EntityFramework.Services;

/// <summary>Hồ sơ mở rộng của pháp nhân, lưu JSON trên entity (<see cref="ILegalEntityProfileEntity.DetailsJson"/>).</summary>
public sealed class LegalEntityProfileService<TLegalEntity>(
    IOrgStructureDbContextProvider dbProvider) : ILegalEntityProfileService
    where TLegalEntity : class, ILegalEntityProfileEntity
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<LegalEntityApiModel?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var entity = await db.Set<TLegalEntity>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : ToApi(entity);
    }

    public async Task<IReadOnlyList<LegalEntityApiModel>> ListAsync(Guid orgId, CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var rows = await db.Set<TLegalEntity>().AsNoTracking()
            .Where(x => x.OrgId == orgId)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.Select(ToApi).ToList();
    }

    public async Task SaveProfileAsync(Guid id, LegalEntityProfilePayload? profile, CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var entity = await db.Set<TLegalEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Không tìm thấy pháp nhân.");

        entity.DetailsJson = profile is null
            ? null
            : JsonSerializer.Serialize(profile, JsonOptions);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public static LegalEntityApiModel ToApi(TLegalEntity entity) =>
        new()
        {
            Id = entity.Id,
            OrgId = entity.OrgId,
            Code = entity.Code,
            Name = entity.Name,
            TaxId = entity.TaxId,
            IsActive = entity.IsActive,
            Profile = Deserialize(entity.DetailsJson),
        };

    public static LegalEntityProfilePayload Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new LegalEntityProfilePayload();
        try
        {
            return JsonSerializer.Deserialize<LegalEntityProfilePayload>(json, JsonOptions) ?? new LegalEntityProfilePayload();
        }
        catch (JsonException)
        {
            return new LegalEntityProfilePayload();
        }
    }
}
