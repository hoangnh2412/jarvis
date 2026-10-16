using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Platform.DDD.Domain.Entities;
using Platform.DDD.Domain.Repositories;
using Platform.DDD.Domain.Services;
using Platform.Modules.ImportExport.Application.Common.Validation;
using Platform.Modules.ImportExport.EntityFramework.Persistence.Mappings;

namespace Platform.Modules.ImportExport.EntityFramework.Stores;

/// <summary>
/// Postgres metadata (<c>MappingsJson</c> + <c>Location</c>) and files on disk.
/// Tenant from <see cref="ICurrentTenant{TTenant}.GetIdAsync"/> (ambient scope or host resolvers), same as Setting module.
/// </summary>
public sealed class DbTemplateStore<TEntity, TUnitOfWork, TTenant>(
    TUnitOfWork unitOfWork,
    TemplateFileResolver fileResolver,
    ICurrentTenant<TTenant> currentTenant,
    ILogger<DbTemplateStore<TEntity, TUnitOfWork, TTenant>> logger) : ITemplateStore
    where TEntity : class, IExcelTemplateEntity, IEntity
    where TUnitOfWork : class, IUnitOfWork
    where TTenant : class, ICurrentTenantIdentity
{
    public async Task<TemplateDefinition?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        await RequireTenantIdAsync(cancellationToken);
        var set = await GetSetAsync(cancellationToken);
        var entity = await set
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code.Trim(), cancellationToken);

        if (entity is null)
            return null;

        var content = await fileResolver.ReadAsync(entity.Location, cancellationToken);
        return ExcelTemplateMapper.ToDefinition(entity, content);
    }

    public async Task SaveAsync(TemplateDefinition template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);
        Validate(template);

        var tenantId = await RequireTenantIdAsync(cancellationToken);
        var code = template.Code.Trim();
        var set = await GetSetAsync(cancellationToken);
        var entity = await set
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == code && x.TenantId == tenantId, cancellationToken);

        if (entity is { IsActive: false })
            throw new TemplateAlreadyExistsException(code);

        var location = template.Location;
        if (template.Content is { Length: > 0 })
        {
            location = await fileResolver.WriteAsync(
                tenantId,
                code,
                template.FileName.Trim(),
                template.Content,
                cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            if (template.Content is not { Length: > 0 } && string.IsNullOrWhiteSpace(location))
                throw new InvalidTemplateException(EngineMessages.TemplateContentRequired);

            entity = (TEntity)RuntimeHelpers.GetUninitializedObject(typeof(TEntity));
            entity.Id = Guid.NewGuid();
            entity.TenantId = tenantId;
            entity.Code = code;
            entity.Name = template.Name.Trim();
            entity.FileName = template.FileName.Trim();
            entity.Location = location;
            entity.MappingsJson = ExcelTemplateMapper.SerializeLayout(
                template.Mappings, template.Sheets, template.SheetName);
            entity.HeaderRow = template.HeaderRow;
            entity.StartRow = template.StartRow;
            entity.RowCount = template.RowCount;
            entity.IsActive = true;
            entity.CreatedAt = now;
            entity.UpdatedAt = now;

            await set.AddAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                EngineLogMessages.TemplateCreated,
                entity.Code,
                template.Mappings.Count,
                entity.FileName);
            return;
        }

        entity.Name = template.Name.Trim();
        entity.FileName = template.FileName.Trim();
        if (!string.IsNullOrWhiteSpace(location))
            entity.Location = location;
        entity.MappingsJson = ExcelTemplateMapper.SerializeLayout(
            template.Mappings, template.Sheets, template.SheetName);
        entity.HeaderRow = template.HeaderRow;
        entity.StartRow = template.StartRow;
        entity.RowCount = template.RowCount;
        entity.UpdatedAt = now;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(EngineLogMessages.TemplateUpdated, entity.Code);
    }

    public async Task DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidTemplateException(EngineMessages.TemplateCodeRequired);

        await RequireTenantIdAsync(cancellationToken);
        var trimmed = code.Trim();
        var set = await GetSetAsync(cancellationToken);
        var entity = await set
            .FirstOrDefaultAsync(x => x.Code == trimmed, cancellationToken)
            ?? throw new TemplateNotFoundException(trimmed);

        entity.SoftDelete();
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(EngineLogMessages.TemplateDeleted, entity.Code);
    }

    public async Task<IReadOnlyList<TemplateDefinition>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        await RequireTenantIdAsync(cancellationToken);
        var set = await GetSetAsync(cancellationToken);
        var rows = await set
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ProjectSummary()
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(r =>
        {
            var (mappings, sheets, sheetName) = ExcelTemplateMapper.DeserializeLayout(r.MappingsJson);
            return new TemplateDefinition
            {
                TenantId = r.TenantId,
                Code = r.Code,
                Name = r.Name,
                FileName = r.FileName,
                Location = r.Location,
                Mappings = mappings,
                Sheets = sheets,
                HeaderRow = r.HeaderRow,
                StartRow = r.StartRow,
                RowCount = r.RowCount,
                SheetName = sheetName,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            };
        });
    }

    private async Task<Guid> RequireTenantIdAsync(CancellationToken cancellationToken)
    {
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty)
            throw new InvalidTemplateException(EngineMessages.TenantRequired);
        return tenantId.Value;
    }

    private async Task<DbSet<TEntity>> GetSetAsync(CancellationToken cancellationToken)
    {
        var storage = await unitOfWork.GetDbContextAsync(cancellationToken);
        return ((DbContext)storage).Set<TEntity>();
    }

    private static void Validate(TemplateDefinition template)
    {
        if (string.IsNullOrWhiteSpace(template.Code))
            throw new InvalidTemplateException(EngineMessages.TemplateCodeRequired);
        ArgumentException.ThrowIfNullOrWhiteSpace(template.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(template.FileName);

        if (template.Sheets.Count > 0)
        {
            foreach (var sheet in template.Sheets)
            {
                if (template.Sheets.Count > 1 && string.IsNullOrWhiteSpace(sheet.SheetName))
                    throw new InvalidTemplateException(EngineMessages.SheetNameRequiredForMultiSheet);
                MappingValidator.Validate(sheet.Mappings, requireAtLeastOne: true);
            }
        }
        else
        {
            MappingValidator.Validate(template.Mappings, requireAtLeastOne: true);
        }

        if (template.HeaderRow < 1)
            throw new InvalidTemplateException(EngineMessages.HeaderRowInvalid());
        if (template.StartRow < 1)
            throw new InvalidTemplateException(EngineMessages.StartRowInvalid());
        if (template.StartRow <= template.HeaderRow)
            throw new InvalidTemplateException(EngineMessages.HeaderRowMustPrecedeStartRow());
    }
}
