using Platform.DDD.Domain.Services;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Orgs.Abstractions;
using Platform.Orgs.Abstractions.Events;
using Platform.Orgs.Hooks;
using Platform.Orgs.Models;
using Platform.Orgs.Persistence;

namespace Platform.Orgs.Services;

/// <summary>
/// CRUD catalog theo ambient tenant. Cột chuẩn do manager ghi; host mở rộng qua <see cref="IOrgCatalogHooks{TOrg}"/>.
/// </summary>
public sealed class OrgManager<TOrg, TTenant> : IOrgManager
    where TOrg : class, IOrgManagementEntity
    where TTenant : class, ICurrentTenantIdentity
{
    private readonly IOrgStore<TOrg> _store;
    private readonly ICurrentTenant<TTenant> _currentTenant;
    private readonly IOrgCatalogHooks<TOrg> _hooks;
    private readonly IOrgCatalogEventPublisher _eventPublisher;
    private readonly IOrgTenantVisibility _visibility;

    public OrgManager(
        IOrgStore<TOrg> store,
        ICurrentTenant<TTenant> currentTenant,
        IOrgCatalogHooks<TOrg> hooks,
        IOrgCatalogEventPublisher eventPublisher,
        IOrgTenantVisibility visibility)
    {
        _store = store;
        _currentTenant = currentTenant;
        _hooks = hooks;
        _eventPublisher = eventPublisher;
        _visibility = visibility;
    }

    public async Task<OrgDescriptor> CreateAsync(CreateOrgInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var code = NormalizeCode(input.Code);
        var name = NormalizeRequired(input.Name, OrgCatalogLimits.NameMaxLength);

        var entity = CreateEntity(Guid.NewGuid());
        ApplyStandardCreateFields(entity, tenantId, code, name, input.ParentId);

        var context = new OrgCatalogCreateHookContext<TOrg>
        {
            TenantId = tenantId,
            Input = input,
            Entity = entity,
        };

        try
        {
            await _hooks.ValidateCreateAsync(context, cancellationToken).ConfigureAwait(false);

            var codeTaken = await _store.CodeExistsAsync(tenantId, code, null, cancellationToken).ConfigureAwait(false);
            if (codeTaken)
                throw new ConflictException(OrgErrorCode.CodeAlreadyExists);

            await EnsureParentAsync(tenantId, entity.Id, input.ParentId, cancellationToken).ConfigureAwait(false);
            await _hooks.PrepareCreateAsync(context, cancellationToken).ConfigureAwait(false);
            ApplyStandardCreateFields(entity, tenantId, code, name, input.ParentId);

            await _store.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            context.Succeeded = true;
            await _hooks.SuccessCreateAsync(context, cancellationToken).ConfigureAwait(false);

            var descriptor = OrgDescriptorMapper.ToDescriptor(entity);
            await _eventPublisher.PublishAsync(
                new OrgCreatedEvent(tenantId, entity.Id, DateTimeOffset.UtcNow, descriptor),
                cancellationToken).ConfigureAwait(false);
            return descriptor;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            await _hooks.ErrorCreateAsync(context, cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally
        {
            await _hooks.FinallyCreateAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<OrgDescriptor> UpdateAsync(
        Guid id,
        UpdateOrgInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var entity = await FindRequiredAsync(tenantId, id, cancellationToken).ConfigureAwait(false);
        var code = NormalizeCode(input.Code);
        var name = NormalizeRequired(input.Name, OrgCatalogLimits.NameMaxLength);
        var beforeCode = entity.Code;
        var beforeName = entity.Name;

        var context = new OrgCatalogUpdateHookContext<TOrg>
        {
            TenantId = tenantId,
            Input = input,
            Entity = entity,
        };

        try
        {
            await _hooks.ValidateUpdateAsync(context, cancellationToken).ConfigureAwait(false);

            var codeTaken = await _store.CodeExistsAsync(tenantId, code, id, cancellationToken).ConfigureAwait(false);
            if (codeTaken)
                throw new ConflictException(OrgErrorCode.CodeAlreadyExists);

            ApplyStandardUpdateFields(entity, tenantId, code, name);
            await _hooks.PrepareUpdateAsync(context, cancellationToken).ConfigureAwait(false);
            ApplyStandardUpdateFields(entity, tenantId, code, name);

            await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            context.Succeeded = true;
            await _hooks.SuccessUpdateAsync(context, cancellationToken).ConfigureAwait(false);

            var descriptor = OrgDescriptorMapper.ToDescriptor(entity);
            await _eventPublisher.PublishAsync(
                new OrgUpdatedEvent(
                    tenantId,
                    entity.Id,
                    DateTimeOffset.UtcNow,
                    Diff(beforeCode, beforeName, null, entity)),
                cancellationToken).ConfigureAwait(false);
            return descriptor;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            await _hooks.ErrorUpdateAsync(context, cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally
        {
            await _hooks.FinallyUpdateAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<OrgDescriptor> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var entity = await FindRequiredAsync(tenantId, id, cancellationToken).ConfigureAwait(false);
        var beforeActive = entity.IsActive;
        var context = new OrgCatalogUpdateHookContext<TOrg>
        {
            TenantId = tenantId,
            Input = null,
            Entity = entity,
        };

        try
        {
            await _hooks.ValidateUpdateAsync(context, cancellationToken).ConfigureAwait(false);

            entity.IsActive = isActive;
            await _hooks.PrepareUpdateAsync(context, cancellationToken).ConfigureAwait(false);
            entity.TenantId = tenantId;

            await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            context.Succeeded = true;
            await _hooks.SuccessUpdateAsync(context, cancellationToken).ConfigureAwait(false);

            var descriptor = OrgDescriptorMapper.ToDescriptor(entity);
            await _eventPublisher.PublishAsync(
                new OrgUpdatedEvent(
                    tenantId,
                    entity.Id,
                    DateTimeOffset.UtcNow,
                    beforeActive == entity.IsActive
                        ? []
                        : [new OrgFieldChange("IsActive", beforeActive.ToString(), entity.IsActive.ToString())]),
                cancellationToken).ConfigureAwait(false);
            return descriptor;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            await _hooks.ErrorUpdateAsync(context, cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally
        {
            await _hooks.FinallyUpdateAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<OrgDescriptor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var entity = await _store.FindTrackedAsync(tenantId, id, cancellationToken).ConfigureAwait(false);
        return entity is null ? null : OrgDescriptorMapper.ToDescriptor(entity);
    }

    public async Task<OrgDescriptor?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var normalized = NormalizeCode(code);
        var entity = await _store.FindByCodeAsync(tenantId, normalized, cancellationToken).ConfigureAwait(false);
        return entity is null ? null : OrgDescriptorMapper.ToDescriptor(entity);
    }

    public async Task<IReadOnlyList<OrgDescriptor>> ListAsync(
        ListOrgsQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var visible = await _visibility.ListVisibleTenantIdsAsync(tenantId, cancellationToken).ConfigureAwait(false);
        var merged = new List<OrgDescriptor>();
        foreach (var visibleTenantId in visible.Distinct())
        {
            var entities = await _store.ListAsync(
                visibleTenantId,
                query?.IsActive,
                query?.Search,
                cancellationToken).ConfigureAwait(false);
            merged.AddRange(entities.Select(OrgDescriptorMapper.ToDescriptor));
        }

        return merged;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _ = id;
        cancellationToken.ThrowIfCancellationRequested();
        throw new ConflictException(OrgErrorCode.HardDeleteDisabled);
    }

    private static IReadOnlyList<OrgFieldChange> Diff(string beforeCode, string beforeName, bool? beforeActive, TOrg entity)
    {
        var changes = new List<OrgFieldChange>();
        if (!string.Equals(beforeCode, entity.Code, StringComparison.Ordinal))
            changes.Add(new OrgFieldChange("Code", beforeCode, entity.Code));
        if (!string.Equals(beforeName, entity.Name, StringComparison.Ordinal))
            changes.Add(new OrgFieldChange("Name", beforeName, entity.Name));
        if (beforeActive is not null && beforeActive != entity.IsActive)
            changes.Add(new OrgFieldChange("IsActive", beforeActive.ToString(), entity.IsActive.ToString()));
        return changes;
    }

    private async Task EnsureParentAsync(
        Guid tenantId,
        Guid orgId,
        Guid? parentId,
        CancellationToken cancellationToken)
    {
        if (parentId is null)
            return;

        if (parentId == Guid.Empty || parentId == orgId)
            throw new BadRequestException(OrgErrorCode.InvalidParent);

        var cursor = parentId;
        var guard = 0;
        while (cursor is Guid currentParentId)
        {
            if (currentParentId == orgId || ++guard > 64)
                throw new BadRequestException(OrgErrorCode.InvalidParent);

            var parent = await _store.FindTrackedAsync(tenantId, currentParentId, cancellationToken)
                .ConfigureAwait(false);
            if (parent is null)
                throw new NotFoundException(OrgErrorCode.NotFound);

            if (parent.TenantId != tenantId)
                throw new BadRequestException(OrgErrorCode.InvalidParent);

            if (!parent.IsActive)
                throw new ConflictException(OrgErrorCode.Inactive);

            cursor = parent.ParentId;
        }
    }

    private static TOrg CreateEntity(Guid id)
    {
        var entity = (TOrg)Activator.CreateInstance(typeof(TOrg))!;
        entity.Id = id;
        return entity;
    }

    private static void ApplyStandardCreateFields(TOrg entity, Guid tenantId, string code, string name, Guid? parentId)
    {
        entity.TenantId = tenantId;
        entity.Code = code;
        entity.Name = name;
        entity.IsActive = true;
        entity.ParentId = parentId;
    }

    private static void ApplyStandardUpdateFields(TOrg entity, Guid tenantId, string code, string name)
    {
        entity.TenantId = tenantId;
        entity.Code = code;
        entity.Name = name;
    }

    private async Task<TOrg> FindRequiredAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _store.FindTrackedAsync(tenantId, id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            throw new NotFoundException(OrgErrorCode.NotFound);

        return entity;
    }

    private async Task<Guid> RequireTenantIdAsync(CancellationToken cancellationToken)
    {
        var tenantId = await _currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty)
            throw new BadRequestException(OrgErrorCode.TenantRequired);

        return tenantId.Value;
    }

    private static string NormalizeCode(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new BadRequestException(OrgErrorCode.InvalidInput);

        var normalized = OrgCatalogLimits.NormalizeCode(trimmed);
        if (normalized.Length > OrgCatalogLimits.CodeMaxLength)
            throw new BadRequestException(OrgErrorCode.InvalidInput);

        return normalized;
    }

    private static string NormalizeRequired(string? value, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        var tooLong = trimmed.Length > maxLength;
        if (trimmed.Length == 0 || tooLong)
            throw new BadRequestException(OrgErrorCode.InvalidInput);

        return trimmed;
    }
}
