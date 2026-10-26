using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.OrgStructure.Models;
using Platform.OrgStructure.Persistence;
using Platform.OrgStructure.Templates;
using Platform.Orgs.Abstractions;

namespace Platform.OrgStructure.Services;

public sealed class OrgStructureManager<TUnit, TLegalEntity, TJobTitle, TPosition> : IOrgStructureManager
    where TUnit : class, IOrganizationalUnitEntity
    where TLegalEntity : class, ILegalEntityEntity
    where TJobTitle : class, IJobTitleEntity
    where TPosition : class, IPositionEntity
{
    private readonly IOrgStructureStore<TUnit, TLegalEntity, TJobTitle, TPosition> _store;
    private readonly IOrgCatalog _orgs;

    public OrgStructureManager(
        IOrgStructureStore<TUnit, TLegalEntity, TJobTitle, TPosition> store,
        IOrgCatalog orgs)
    {
        _store = store;
        _orgs = orgs;
    }

    public async Task<OrganizationalUnitDescriptor> CreateUnitAsync(
        CreateOrganizationalUnitInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        await _orgs.ValidateForAssignmentAsync(input.OrgId, input.TenantId, cancellationToken).ConfigureAwait(false);

        var code = NormalizeCode(input.Code);
        var name = NormalizeRequired(input.Name, OrgStructureLimits.NameMaxLength);
        var unitType = NormalizeUnitType(input.UnitType);
        var units = await _store.ListUnitsAsync(input.OrgId, cancellationToken).ConfigureAwait(false);
        if (units.Any(unit => unit.Code == code))
            throw new ConflictException(OrgStructureErrorCode.CodeAlreadyExists);

        var parent = await RequireParentAsync(units, input.ParentId, input.OrgId, cancellationToken).ConfigureAwait(false);
        await RequireLegalEntityAsync(input.OrgId, input.LegalEntityId, requireActive: true, cancellationToken)
            .ConfigureAwait(false);

        var entity = Create<TUnit>();
        entity.OrgId = input.OrgId;
        entity.TenantId = input.TenantId;
        entity.ParentId = parent?.Id;
        entity.Code = code;
        entity.Name = name;
        entity.UnitType = unitType;
        entity.SortOrder = input.SortOrder;
        entity.IsActive = true;
        entity.LegalEntityId = input.LegalEntityId;
        entity.Path = BuildPath(parent?.Path, code);

        await _store.AddUnitAsync(entity, cancellationToken).ConfigureAwait(false);
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToUnit(entity);
    }

    public async Task<OrganizationalUnitDescriptor> UpdateUnitAsync(
        Guid id,
        UpdateOrganizationalUnitInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var entity = await FindUnitRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var code = NormalizeCode(input.Code);
        var name = NormalizeRequired(input.Name, OrgStructureLimits.NameMaxLength);
        var unitType = NormalizeUnitType(input.UnitType);
        var units = await _store.ListUnitsAsync(entity.OrgId, cancellationToken).ConfigureAwait(false);
        if (units.Any(unit => unit.Id != id && unit.Code == code))
            throw new ConflictException(OrgStructureErrorCode.CodeAlreadyExists);

        await RequireLegalEntityAsync(entity.OrgId, input.LegalEntityId, requireActive: true, cancellationToken)
            .ConfigureAwait(false);

        var parentPath = entity.ParentId is null
            ? null
            : units.First(unit => unit.Id == entity.ParentId).Path;
        var newPath = BuildPath(parentPath, code);
        RewritePaths(units, entity, newPath);
        entity.Code = code;
        entity.Name = name;
        entity.UnitType = unitType;
        entity.SortOrder = input.SortOrder;
        entity.LegalEntityId = input.LegalEntityId;
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToUnit(entity);
    }

    public async Task<OrganizationalUnitDescriptor> MoveUnitAsync(
        Guid id,
        Guid? parentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindUnitRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var units = await _store.ListUnitsAsync(entity.OrgId, cancellationToken).ConfigureAwait(false);
        if (parentId == id)
            throw new BadRequestException(OrgStructureErrorCode.InvalidParent);

        TUnit? parent = null;
        if (parentId is Guid parentValue)
        {
            parent = units.FirstOrDefault(unit => unit.Id == parentValue)
                ?? await _store.FindUnitAsync(parentValue, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException(OrgStructureErrorCode.NotFound);
            if (parent.OrgId != entity.OrgId)
                throw new BadRequestException(OrgStructureErrorCode.InvalidParent);
            if (!parent.IsActive)
                throw new ConflictException(OrgStructureErrorCode.ParentInactive);
            if (parent.Path == entity.Path || parent.Path.StartsWith(entity.Path + "/", StringComparison.Ordinal))
                throw new BadRequestException(OrgStructureErrorCode.InvalidParent);
        }

        RewritePaths(units, entity, BuildPath(parent?.Path, entity.Code));
        entity.ParentId = parent?.Id;
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToUnit(entity);
    }

    public async Task<OrganizationalUnitDescriptor> SetUnitActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindUnitRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        if (!isActive)
        {
            var units = await _store.ListUnitsAsync(entity.OrgId, cancellationToken).ConfigureAwait(false);
            var branch = units.Where(unit =>
                unit.Id == entity.Id || unit.Path.StartsWith(entity.Path + "/", StringComparison.Ordinal));
            var branchIds = branch.Select(unit => unit.Id).ToHashSet();
            var positions = await _store.ListPositionsAsync(entity.OrgId, cancellationToken).ConfigureAwait(false);
            if (positions.Any(position => position.IsActive && branchIds.Contains(position.OrganizationalUnitId)))
                throw new ConflictException(OrgStructureErrorCode.HasActivePositions);
        }
        else if (entity.ParentId is Guid parentId)
        {
            var parent = await FindUnitRequiredAsync(parentId, cancellationToken).ConfigureAwait(false);
            if (!parent.IsActive)
                throw new ConflictException(OrgStructureErrorCode.ParentInactive);
        }

        entity.IsActive = isActive;
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToUnit(entity);
    }

    public async Task<OrganizationalUnitDescriptor> GetUnitAsync(Guid id, CancellationToken cancellationToken = default) =>
        ToUnit(await FindUnitRequiredAsync(id, cancellationToken).ConfigureAwait(false));

    public async Task<IReadOnlyList<OrganizationalUnitDescriptor>> ListUnitsAsync(
        ListOrganizationalUnitsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var units = await _store.ListUnitsAsync(query.OrgId, cancellationToken).ConfigureAwait(false);
        IEnumerable<TUnit> rows = units;
        if (!string.IsNullOrWhiteSpace(query.UnitType))
        {
            var unitType = NormalizeUnitType(query.UnitType);
            rows = rows.Where(unit => unit.UnitType == unitType);
        }

        if (query.LegalEntityId is Guid legalEntityId)
            rows = rows.Where(unit => unit.LegalEntityId == legalEntityId);
        if (query.IsActive is bool active)
            rows = rows.Where(unit => unit.IsActive == active);

        return OrderUnitsForList(rows).Select(ToUnit).ToArray();
    }

    public async Task<IReadOnlyList<OrganizationalUnitDescriptor>> GetDescendantsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindUnitRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var units = await _store.ListUnitsAsync(entity.OrgId, cancellationToken).ConfigureAwait(false);
        var prefix = entity.Path + "/";
        return units
            .Where(unit => unit.Path.StartsWith(prefix, StringComparison.Ordinal))
            .OrderBy(unit => unit.Path, StringComparer.Ordinal)
            .Select(ToUnit)
            .ToArray();
    }

    public async Task<LegalEntityDescriptor> CreateLegalEntityAsync(
        CreateLegalEntityInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var code = NormalizeCode(input.Code);
        var name = NormalizeRequired(input.Name, OrgStructureLimits.NameMaxLength);
        var taxId = NormalizeOptional(input.TaxId, OrgStructureLimits.TaxIdMaxLength);
        var rows = await _store.ListLegalEntitiesAsync(input.OrgId, cancellationToken).ConfigureAwait(false);
        if (rows.Any(row => row.Code == code))
            throw new ConflictException(OrgStructureErrorCode.CodeAlreadyExists);

        var entity = Create<TLegalEntity>();
        entity.OrgId = input.OrgId;
        entity.Code = code;
        entity.Name = name;
        entity.TaxId = taxId;
        entity.IsActive = true;
        await _store.AddLegalEntityAsync(entity, cancellationToken).ConfigureAwait(false);
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToLegal(entity);
    }

    public async Task<LegalEntityDescriptor> UpdateLegalEntityAsync(
        Guid id,
        UpdateLegalEntityInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var entity = await FindLegalRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var code = NormalizeCode(input.Code);
        var rows = await _store.ListLegalEntitiesAsync(entity.OrgId, cancellationToken).ConfigureAwait(false);
        if (rows.Any(row => row.Id != id && row.Code == code))
            throw new ConflictException(OrgStructureErrorCode.CodeAlreadyExists);

        entity.Code = code;
        entity.Name = NormalizeRequired(input.Name, OrgStructureLimits.NameMaxLength);
        entity.TaxId = NormalizeOptional(input.TaxId, OrgStructureLimits.TaxIdMaxLength);
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToLegal(entity);
    }

    public async Task<LegalEntityDescriptor> SetLegalEntityActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindLegalRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        if (!isActive)
        {
            var units = await _store.ListUnitsAsync(entity.OrgId, cancellationToken).ConfigureAwait(false);
            if (units.Any(unit => unit.IsActive && unit.LegalEntityId == id))
                throw new ConflictException(OrgStructureErrorCode.InvalidLegalEntity);
        }

        entity.IsActive = isActive;
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToLegal(entity);
    }

    public async Task<IReadOnlyList<LegalEntityDescriptor>> ListLegalEntitiesAsync(
        Guid orgId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _store.ListLegalEntitiesAsync(orgId, cancellationToken).ConfigureAwait(false);
        return rows.OrderBy(row => row.Code, StringComparer.Ordinal).Select(ToLegal).ToArray();
    }

    public async Task<JobTitleDescriptor> CreateJobTitleAsync(
        CreateJobTitleInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var code = NormalizeCode(input.Code);
        var rows = await _store.ListJobTitlesAsync(input.OrgId, cancellationToken).ConfigureAwait(false);
        if (rows.Any(row => row.Code == code))
            throw new ConflictException(OrgStructureErrorCode.CodeAlreadyExists);

        var entity = Create<TJobTitle>();
        entity.OrgId = input.OrgId;
        entity.Code = code;
        entity.Name = NormalizeRequired(input.Name, OrgStructureLimits.NameMaxLength);
        await _store.AddJobTitleAsync(entity, cancellationToken).ConfigureAwait(false);
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToJob(entity);
    }

    public async Task<JobTitleDescriptor> UpdateJobTitleAsync(
        Guid id,
        UpdateJobTitleInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var entity = await FindJobRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var code = NormalizeCode(input.Code);
        var rows = await _store.ListJobTitlesAsync(entity.OrgId, cancellationToken).ConfigureAwait(false);
        if (rows.Any(row => row.Id != id && row.Code == code))
            throw new ConflictException(OrgStructureErrorCode.CodeAlreadyExists);

        entity.Code = code;
        entity.Name = NormalizeRequired(input.Name, OrgStructureLimits.NameMaxLength);
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToJob(entity);
    }

    public async Task<IReadOnlyList<JobTitleDescriptor>> ListJobTitlesAsync(
        Guid orgId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _store.ListJobTitlesAsync(orgId, cancellationToken).ConfigureAwait(false);
        return rows.OrderBy(row => row.Code, StringComparer.Ordinal).Select(ToJob).ToArray();
    }

    public async Task<PositionDescriptor> CreatePositionAsync(
        CreatePositionInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var unit = await FindUnitRequiredAsync(input.OrganizationalUnitId, cancellationToken).ConfigureAwait(false);
        if (unit.OrgId != input.OrgId || !unit.IsActive)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);
        await RequireJobTitleAsync(input.OrgId, input.JobTitleId, cancellationToken).ConfigureAwait(false);

        var entity = Create<TPosition>();
        entity.OrgId = input.OrgId;
        entity.OrganizationalUnitId = input.OrganizationalUnitId;
        entity.JobTitleId = input.JobTitleId;
        entity.IsActive = true;
        var positions = await _store.ListPositionsAsync(input.OrgId, cancellationToken).ConfigureAwait(false);
        EnsureReportsTo(positions, entity.Id, input.ReportsToPositionId, input.OrgId);
        entity.ReportsToPositionId = input.ReportsToPositionId;
        await _store.AddPositionAsync(entity, cancellationToken).ConfigureAwait(false);
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToPosition(entity);
    }

    public async Task<PositionDescriptor> UpdatePositionAsync(
        Guid id,
        UpdatePositionInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var entity = await FindPositionRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        await RequireJobTitleAsync(entity.OrgId, input.JobTitleId, cancellationToken).ConfigureAwait(false);
        var positions = await _store.ListPositionsAsync(entity.OrgId, cancellationToken).ConfigureAwait(false);
        EnsureReportsTo(positions, entity.Id, input.ReportsToPositionId, entity.OrgId);
        entity.JobTitleId = input.JobTitleId;
        entity.ReportsToPositionId = input.ReportsToPositionId;
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToPosition(entity);
    }

    public async Task<PositionDescriptor> SetPositionActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindPositionRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        entity.IsActive = isActive;
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToPosition(entity);
    }

    public async Task<IReadOnlyList<PositionDescriptor>> ListPositionsAsync(
        Guid orgId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _store.ListPositionsAsync(orgId, cancellationToken).ConfigureAwait(false);
        return rows.Select(ToPosition).ToArray();
    }

    public OrgStructureTemplatePreview PreviewTemplate(string templateCode, string rootName)
    {
        try
        {
            if (string.Equals(templateCode, OrgStructureLimits.SimpleCompanyTemplate, StringComparison.Ordinal))
                return SimpleCompanyTemplate.Preview(rootName);
            if (string.Equals(templateCode, OrgStructureLimits.CorporationTemplate, StringComparison.Ordinal))
                return CorporationTemplate.Preview(rootName);
            if (string.Equals(templateCode, OrgStructureLimits.HoldingGroupTemplate, StringComparison.Ordinal))
                return HoldingGroupTemplate.Preview(rootName);

            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);
        }
        catch (ArgumentException)
        {
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);
        }
    }

    public async Task<IReadOnlyList<OrganizationalUnitDescriptor>> ApplyTemplateAsync(
        ApplyOrgStructureTemplateInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.Confirmed)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);

        await _orgs.ValidateForAssignmentAsync(input.OrgId, input.TenantId, cancellationToken).ConfigureAwait(false);
        var preview = PreviewTemplate(input.TemplateCode, input.RootName);
        if (!SamePreview(preview.Nodes, input.Nodes))
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);

        var existing = await _store.ListUnitsAsync(input.OrgId, cancellationToken).ConfigureAwait(false);
        if (existing.Count > 0)
            throw new ConflictException(OrgStructureErrorCode.TemplateNotEmpty);

        var created = new List<TUnit>(preview.Nodes.Count);
        foreach (var node in preview.Nodes)
        {
            var parent = node.ParentCode is null
                ? null
                : created.First(unit => unit.Code == node.ParentCode);
            var entity = Create<TUnit>();
            entity.OrgId = input.OrgId;
            entity.TenantId = input.TenantId;
            entity.ParentId = parent?.Id;
            entity.Code = node.Code;
            entity.Name = node.Name;
            entity.UnitType = node.UnitType;
            entity.SortOrder = 0;
            entity.IsActive = true;
            entity.Path = node.Path;
            await _store.AddUnitAsync(entity, cancellationToken).ConfigureAwait(false);
            created.Add(entity);
        }

        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return created.Select(ToUnit).ToArray();
    }

    private async Task<TUnit?> RequireParentAsync(
        IReadOnlyList<TUnit> units,
        Guid? parentId,
        Guid orgId,
        CancellationToken cancellationToken)
    {
        if (parentId is not Guid value)
            return null;

        var parent = units.FirstOrDefault(unit => unit.Id == value)
            ?? await FindUnitRequiredAsync(value, cancellationToken).ConfigureAwait(false);
        if (parent.OrgId != orgId)
            throw new BadRequestException(OrgStructureErrorCode.InvalidParent);
        if (!parent.IsActive)
            throw new ConflictException(OrgStructureErrorCode.ParentInactive);
        return parent;
    }

    private async Task RequireLegalEntityAsync(
        Guid orgId,
        Guid? legalEntityId,
        bool requireActive,
        CancellationToken cancellationToken)
    {
        if (legalEntityId is not Guid value)
            return;

        var entity = await FindLegalRequiredAsync(value, cancellationToken).ConfigureAwait(false);
        if (entity.OrgId != orgId || (requireActive && !entity.IsActive))
            throw new BadRequestException(OrgStructureErrorCode.InvalidLegalEntity);
    }

    private async Task RequireJobTitleAsync(Guid orgId, Guid? jobTitleId, CancellationToken cancellationToken)
    {
        if (jobTitleId is not Guid value)
            return;

        var entity = await FindJobRequiredAsync(value, cancellationToken).ConfigureAwait(false);
        if (entity.OrgId != orgId)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);
    }

    private static void EnsureReportsTo(
        IReadOnlyList<TPosition> positions,
        Guid selfId,
        Guid? reportsToId,
        Guid orgId)
    {
        if (reportsToId is not Guid cursor)
            return;
        if (cursor == selfId)
            throw new BadRequestException(OrgStructureErrorCode.InvalidReportsTo);

        var guard = 0;
        while (true)
        {
            var current = positions.FirstOrDefault(position => position.Id == cursor);
            if (current is null || current.OrgId != orgId)
                throw new BadRequestException(OrgStructureErrorCode.InvalidReportsTo);
            if (current.ReportsToPositionId is not Guid next)
                return;
            if (next == selfId)
                throw new BadRequestException(OrgStructureErrorCode.InvalidReportsTo);
            cursor = next;
            if (++guard > 64)
                throw new BadRequestException(OrgStructureErrorCode.InvalidReportsTo);
        }
    }

    private async Task<TUnit> FindUnitRequiredAsync(Guid id, CancellationToken cancellationToken) =>
        await _store.FindUnitAsync(id, cancellationToken).ConfigureAwait(false)
        ?? throw new NotFoundException(OrgStructureErrorCode.NotFound);

    private async Task<TLegalEntity> FindLegalRequiredAsync(Guid id, CancellationToken cancellationToken) =>
        await _store.FindLegalEntityAsync(id, cancellationToken).ConfigureAwait(false)
        ?? throw new NotFoundException(OrgStructureErrorCode.NotFound);

    private async Task<TJobTitle> FindJobRequiredAsync(Guid id, CancellationToken cancellationToken) =>
        await _store.FindJobTitleAsync(id, cancellationToken).ConfigureAwait(false)
        ?? throw new NotFoundException(OrgStructureErrorCode.NotFound);

    private async Task<TPosition> FindPositionRequiredAsync(Guid id, CancellationToken cancellationToken) =>
        await _store.FindPositionAsync(id, cancellationToken).ConfigureAwait(false)
        ?? throw new NotFoundException(OrgStructureErrorCode.NotFound);

    private static void RewritePaths(IReadOnlyList<TUnit> units, TUnit node, string newPath)
    {
        var oldPath = node.Path;
        if (string.Equals(oldPath, newPath, StringComparison.Ordinal))
            return;

        foreach (var unit in units)
        {
            if (unit.Id == node.Id || oldPath.Length == 0)
                continue;
            if (unit.Path.StartsWith(oldPath + "/", StringComparison.Ordinal))
                unit.Path = newPath + unit.Path[oldPath.Length..];
        }

        node.Path = newPath;
    }

    private static string BuildPath(string? parentPath, string code)
    {
        var path = string.IsNullOrEmpty(parentPath) ? "/" + code : parentPath + "/" + code;
        if (path.Length > OrgStructureLimits.PathMaxLength)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);
        return path;
    }

    private static bool SamePreview(
        IReadOnlyList<OrgStructureTemplateNode> expected,
        IReadOnlyList<OrgStructureTemplateNode> actual)
    {
        if (actual is null || expected.Count != actual.Count)
            return false;

        var left = expected.OrderBy(node => node.Code, StringComparer.Ordinal).ToArray();
        var right = actual.OrderBy(node => node.Code, StringComparer.Ordinal).ToArray();
        for (var index = 0; index < left.Length; index++)
        {
            if (left[index] != right[index])
                return false;
        }

        return true;
    }

    private static T Create<T>()
        where T : class
    {
        var entity = (T)Activator.CreateInstance(typeof(T))!;
        if (entity is IOrganizationalUnitEntity unit)
            unit.Id = Guid.NewGuid();
        else if (entity is ILegalEntityEntity legal)
            legal.Id = Guid.NewGuid();
        else if (entity is IJobTitleEntity job)
            job.Id = Guid.NewGuid();
        else if (entity is IPositionEntity position)
            position.Id = Guid.NewGuid();
        return entity;
    }

    private static string NormalizeCode(string? value)
    {
        var normalized = OrgStructureLimits.NormalizeCode(value ?? string.Empty);
        if (normalized.Length == 0 || normalized.Length > OrgStructureLimits.CodeMaxLength)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);
        return normalized;
    }

    private static string NormalizeRequired(string? value, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 || trimmed.Length > maxLength)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);
        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return null;
        if (trimmed.Length > maxLength)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);
        return trimmed;
    }

    private static string NormalizeUnitType(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 || trimmed.Length > OrgStructureLimits.UnitTypeMaxLength)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput);
        return trimmed;
    }

    private static IReadOnlyList<TUnit> OrderUnitsForList(IEnumerable<TUnit> rows)
    {
        var list = rows.ToList();
        if (list.Count == 0)
            return list;

        var byParent = list.ToLookup(unit => unit.ParentId);
        var ordered = new List<TUnit>(list.Count);
        var visited = new HashSet<Guid>();

        void Walk(Guid? parentId)
        {
            foreach (var unit in byParent[parentId]
                         .OrderBy(unit => unit.SortOrder)
                         .ThenBy(unit => unit.Name, StringComparer.Ordinal))
            {
                if (!visited.Add(unit.Id))
                    continue;
                ordered.Add(unit);
                Walk(unit.Id);
            }
        }

        Walk(null);
        foreach (var unit in list)
        {
            if (visited.Add(unit.Id))
                ordered.Add(unit);
        }

        return ordered;
    }

    private static OrganizationalUnitDescriptor ToUnit(TUnit entity) => new()
    {
        Id = entity.Id,
        OrgId = entity.OrgId,
        TenantId = entity.TenantId,
        ParentId = entity.ParentId,
        Code = entity.Code,
        Name = entity.Name,
        UnitType = entity.UnitType,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive,
        LegalEntityId = entity.LegalEntityId,
        Path = entity.Path,
    };

    private static LegalEntityDescriptor ToLegal(TLegalEntity entity) => new()
    {
        Id = entity.Id,
        OrgId = entity.OrgId,
        Code = entity.Code,
        Name = entity.Name,
        TaxId = entity.TaxId,
        IsActive = entity.IsActive,
    };

    private static JobTitleDescriptor ToJob(TJobTitle entity) => new()
    {
        Id = entity.Id,
        OrgId = entity.OrgId,
        Code = entity.Code,
        Name = entity.Name,
    };

    private static PositionDescriptor ToPosition(TPosition entity) => new()
    {
        Id = entity.Id,
        OrgId = entity.OrgId,
        OrganizationalUnitId = entity.OrganizationalUnitId,
        JobTitleId = entity.JobTitleId,
        ReportsToPositionId = entity.ReportsToPositionId,
        IsActive = entity.IsActive,
    };
}
