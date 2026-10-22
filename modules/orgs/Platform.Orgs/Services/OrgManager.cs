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
    where TOrg : class, IOrgCatalogEntity
    where TTenant : class, ICurrentTenantIdentity
{
    private readonly IOrgStore<TOrg> _store;
    private readonly ICurrentTenant<TTenant> _currentTenant;
    private readonly IOrgCatalogHooks<TOrg> _hooks;
    private readonly IOrgCatalogEventPublisher _eventPublisher;

    public OrgManager(
        IOrgStore<TOrg> store,
        ICurrentTenant<TTenant> currentTenant,
        IOrgCatalogHooks<TOrg> hooks,
        IOrgCatalogEventPublisher eventPublisher)
    {
        _store = store;
        _currentTenant = currentTenant;
        _hooks = hooks;
        _eventPublisher = eventPublisher;
    }

    public async Task<OrgDescriptor> CreateAsync(CreateOrgInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var code = NormalizeCode(input.Code);
        var name = NormalizeRequired(input.Name, OrgCatalogLimits.NameMaxLength);

        var entity = CreateEntity(Guid.NewGuid());
        ApplyStandardCreateFields(entity, tenantId, code, name);

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

            await _hooks.PrepareCreateAsync(context, cancellationToken).ConfigureAwait(false);
            ApplyStandardCreateFields(entity, tenantId, code, name);

            await _store.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var descriptor = OrgDescriptorMapper.ToDescriptor(entity);
            await _eventPublisher.PublishAsync(
                new OrgCreatedEvent(tenantId, entity.Id, DateTimeOffset.UtcNow, descriptor),
                cancellationToken).ConfigureAwait(false);

            context.Succeeded = true;
            await _hooks.SuccessCreateAsync(context, cancellationToken).ConfigureAwait(false);
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

            var descriptor = OrgDescriptorMapper.ToDescriptor(entity);
            await _eventPublisher.PublishAsync(
                new OrgUpdatedEvent(tenantId, entity.Id, DateTimeOffset.UtcNow, descriptor),
                cancellationToken).ConfigureAwait(false);

            context.Succeeded = true;
            await _hooks.SuccessUpdateAsync(context, cancellationToken).ConfigureAwait(false);
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

            var descriptor = OrgDescriptorMapper.ToDescriptor(entity);
            await _eventPublisher.PublishAsync(
                new OrgUpdatedEvent(tenantId, entity.Id, DateTimeOffset.UtcNow, descriptor),
                cancellationToken).ConfigureAwait(false);

            context.Succeeded = true;
            await _hooks.SuccessUpdateAsync(context, cancellationToken).ConfigureAwait(false);
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
        var entities = await _store.ListAsync(
            tenantId,
            query?.IsActive,
            query?.Search,
            cancellationToken).ConfigureAwait(false);

        return entities.Select(OrgDescriptorMapper.ToDescriptor).ToArray();
    }

    private static TOrg CreateEntity(Guid id)
    {
        var entity = (TOrg)Activator.CreateInstance(typeof(TOrg))!;
        entity.Id = id;
        return entity;
    }

    private static void ApplyStandardCreateFields(TOrg entity, Guid tenantId, string code, string name)
    {
        entity.TenantId = tenantId;
        entity.Code = code;
        entity.Name = name;
        entity.IsActive = true;
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
