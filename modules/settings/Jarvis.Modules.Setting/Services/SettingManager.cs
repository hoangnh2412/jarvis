using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Jarvis.Caching;
using Jarvis.Common.Encryption;
using Jarvis.DDD.Domain.Entities;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.DDD.Domain.Services;
using Jarvis.DDD.Domain.Shared.ExceptionHandling;
using Jarvis.Modules.Setting.Definitions;
using Jarvis.Modules.Setting.Models;
using Jarvis.Modules.Setting.Validation;

namespace Jarvis.Modules.Setting.Services;

/// <summary>
/// Triển khai mặc định của <see cref="ISettingManager"/>.
/// Kết hợp Library (definition) + cache Tenant+Key + mã hóa at-rest + CRUD qua Unit of Work.
/// </summary>
/// <remarks>
/// Quy ước quan trọng:
/// <list type="bullet">
/// <item>Cache lưu đúng giá trị persisted (ciphertext với secret) — không cache plaintext.</item>
/// <item>Caller luôn nhận plaintext sau khi decrypt.</item>
/// <item>Host phải có tenant context qua <see cref="ICurrentTenant{TTenant}"/> trước mọi thao tác đọc/ghi.</item>
/// </list>
/// </remarks>
/// <typeparam name="TSetting">Entity concrete của host (implement <see cref="ISettingEntity"/>).</typeparam>
/// <typeparam name="TUnitOfWork">Unit of Work host cung cấp repository.</typeparam>
/// <typeparam name="TTenant">Profile tenant của host (implement <see cref="ICurrentTenantIdentity"/>).</typeparam>
public sealed class SettingManager<TSetting, TUnitOfWork, TTenant> : ISettingManager
    where TSetting : class, ISettingEntity
    where TUnitOfWork : class, IUnitOfWork
    where TTenant : class, ICurrentTenantIdentity
{
    /// <summary>Tên cố định trong <c>Cache:Items</c> — host phải khai báo entry này.</summary>
    private const string CacheItemName = "Setting";

    private readonly TUnitOfWork _unitOfWork;
    private readonly ICurrentTenant<TTenant> _currentTenant;
    private readonly ISettingDefinitionRegistry _registry;
    private readonly ICacheService _cache;
    private readonly EncryptionOptions _encryptionOptions;

    /// <summary>
    /// Tạo manager với các phụ thuộc: UoW, current tenant, Library, cache, encryption options.
    /// </summary>
    public SettingManager(
        TUnitOfWork unitOfWork,
        ICurrentTenant<TTenant> currentTenant,
        ISettingDefinitionRegistry registry,
        ICacheService cache,
        IOptions<EncryptionOptions> encryptionOptions)
    {
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _registry = registry;
        _cache = cache;
        _encryptionOptions = encryptionOptions.Value;
    }

    public IReadOnlyList<SettingGroupDefinition> GetGroups() => _registry.GetGroups();

    public IReadOnlyList<SettingDefinition> GetDefinitions(string? group = null) => _registry.GetSettings(group);

    public async Task<SettingModel?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var canonicalKey = ResolveCanonicalKey(key);
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var cacheParam = CreateCacheParam(tenantId, canonicalKey);

        // Cache miss → đọc DB; cache hit → lấy persisted model (có thể là ciphertext).
        var persisted = await _cache.GetOrSetAsync(
            cacheParam,
            async ct =>
            {
                var repo = await _unitOfWork.GetRepositoryAsync<IRepository<TSetting>>(ct).ConfigureAwait(false);
                var entity = (await repo.ListAsync(x => x.Key == canonicalKey, ct).ConfigureAwait(false)).FirstOrDefault();
                // null! : GetOrSetAsync bỏ qua null — không ghi cache cho "chưa có bản ghi".
                return entity is null ? null! : ToPersistedModel(entity);
            },
            cancellationToken).ConfigureAwait(false);

        // Decrypt chỉ sau khi lấy từ cache/DB — caller không bao giờ thấy ciphertext.
        return persisted is null ? null : ToPlaintextModel(persisted);
    }

    public async Task<IReadOnlyList<SettingModel>> GetByGroupAsync(string group, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);

        var canonicalGroup = ResolveCanonicalGroup(group);
        await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var repo = await _unitOfWork.GetRepositoryAsync<IRepository<TSetting>>(cancellationToken).ConfigureAwait(false);
        var entities = await repo.ListAsync(x => x.Group == canonicalGroup, cancellationToken).ConfigureAwait(false);

        // Không cache theo group — luôn đọc DB rồi decrypt từng Value.
        return entities
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => ToModel(x))
            .ToList();
    }

    public async Task<IReadOnlyList<SettingFormItemModel>> GetFormAsync(string group, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);

        var canonicalGroup = ResolveCanonicalGroup(group);
        var definitions = _registry.GetSettings(canonicalGroup);
        if (definitions.Count == 0)
            throw new NotFoundException(SettingErrorCode.DefinitionNotFound, $"Setting group '{group}' has no registered definitions.");

        await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var repo = await _unitOfWork.GetRepositoryAsync<IRepository<TSetting>>(cancellationToken).ConfigureAwait(false);
        var entities = await repo.ListAsync(x => x.Group == canonicalGroup, cancellationToken).ConfigureAwait(false);
        var byKey = entities.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        // Library là source of truth cho metadata form; DB chỉ bổ sung Value đã lưu.
        var result = new List<SettingFormItemModel>(definitions.Count);
        foreach (var definition in definitions)
        {
            if (byKey.TryGetValue(definition.Key, out var entity))
            {
                var model = ToModel(entity, definition);
                result.Add(new SettingFormItemModel
                {
                    Group = definition.Group,
                    Key = definition.Key,
                    Name = definition.Name,
                    Type = definition.Type,
                    Options = definition.Options,
                    Description = definition.Description,
                    DefaultValue = definition.DefaultValue,
                    IsReadOnly = definition.IsReadOnly,
                    IsEncrypted = definition.IsEncrypted,
                    Value = model.Value,
                    IsPersisted = true,
                });
            }
            else
            {
                // Chưa có row → hiển thị DefaultValue, đánh dấu chưa persist.
                result.Add(new SettingFormItemModel
                {
                    Group = definition.Group,
                    Key = definition.Key,
                    Name = definition.Name,
                    Type = definition.Type,
                    Options = definition.Options,
                    Description = definition.Description,
                    DefaultValue = definition.DefaultValue,
                    IsReadOnly = definition.IsReadOnly,
                    IsEncrypted = definition.IsEncrypted,
                    Value = definition.DefaultValue ?? string.Empty,
                    IsPersisted = false,
                });
            }
        }

        return result;
    }

    public async Task<SettingModel> CreateAsync(string key, string? value = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var definition = _registry.GetSetting(key)
            ?? throw new NotFoundException(SettingErrorCode.DefinitionNotFound, $"Setting definition '{key}' is not registered.");

        var canonicalKey = definition.Key;
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var repo = await _unitOfWork.GetRepositoryAsync<IRepository<TSetting>>(cancellationToken).ConfigureAwait(false);

        // Kiểm tra trùng Key trước khi insert (unique index host vẫn là lớp bảo vệ cuối).
        if (await repo.AnyAsync(x => x.Key == canonicalKey, cancellationToken).ConfigureAwait(false))
            throw new ConflictException(SettingErrorCode.KeyAlreadyExists, $"Setting key '{canonicalKey}' already exists.");

        var plaintext = value ?? definition.DefaultValue ?? string.Empty;
        EnsureSecretCanPersist(definition, plaintext);
        SettingValueValidator.Validate(definition, plaintext);

        var entity = CreateEntity();
        entity.Id = Guid.CreateVersion7();
        entity.TenantId = tenantId;
        ApplyDefinitionMetadata(entity, definition);
        entity.Value = PersistValue(plaintext, definition);

        try
        {
            await repo.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Race với request khác: unique index thắng → chuẩn hóa thành ConflictException.
            if (await repo.AnyAsync(x => x.Key == canonicalKey, cancellationToken).ConfigureAwait(false))
            {
                throw new ConflictException(
                    SettingErrorCode.KeyAlreadyExists,
                    $"Setting key '{canonicalKey}' already exists.");
            }
            throw;
        }

        await DeleteCacheAsync(tenantId, canonicalKey, cancellationToken).ConfigureAwait(false);
        return ToModel(entity, definition);
    }

    public async Task<SettingModel> GetOrCreateAsync(string key, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return existing;

        try
        {
            return await CreateAsync(key, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (ConflictException)
        {
            // Request khác đã tạo cùng Key — đọc lại row thắng race, không ném lỗi cho caller.
            var createdByOther = await GetAsync(key, cancellationToken).ConfigureAwait(false);
            if (createdByOther is not null)
                return createdByOther;

            throw;
        }
    }

    public async Task<SettingModel> UpdateAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        // Key phải còn trong Library — không cho cập nhật orphan row khi definition đã bị gỡ.
        var definition = _registry.GetSetting(key)
            ?? throw new NotFoundException(SettingErrorCode.DefinitionNotFound, $"Setting definition '{key}' is not registered.");

        var canonicalKey = definition.Key;
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var repo = await _unitOfWork.GetRepositoryAsync<IRepository<TSetting>>(cancellationToken).ConfigureAwait(false);
        var entity = await repo.GetByIdAsync(x => x.Key == canonicalKey, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(SettingErrorCode.NotFound, $"Setting '{canonicalKey}' was not found.");

        if (entity.IsReadOnly || definition.IsReadOnly)
            throw new BadRequestException(SettingErrorCode.ReadOnly, $"Setting '{canonicalKey}' is read-only and cannot be updated.");

        EnsureSecretCanPersist(definition, value);
        SettingValueValidator.Validate(definition, value);
        ApplyDefinitionMetadata(entity, definition);
        entity.Value = PersistValue(value, definition);
        await repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await DeleteCacheAsync(tenantId, canonicalKey, cancellationToken).ConfigureAwait(false);

        return ToModel(entity, definition);
    }

    public async Task<IReadOnlyList<SettingModel>> SaveGroupAsync(
        string group,
        IReadOnlyDictionary<string, string> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count == 0)
            throw new ArgumentException("At least one setting value is required.", nameof(values));

        var canonicalGroup = ResolveCanonicalGroup(group);
        var definitions = _registry.GetSettings(canonicalGroup);
        if (definitions.Count == 0)
            throw new NotFoundException(SettingErrorCode.DefinitionNotFound, $"Setting group '{group}' has no registered definitions.");

        // Validate mọi Key gửi lên thuộc đúng Group và có trong Library trước khi ghi.
        var definitionByKey = definitions.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        foreach (var key in values.Keys)
        {
            if (!definitionByKey.TryGetValue(key, out var definition))
            {
                throw new BadRequestException(
                    SettingErrorCode.DefinitionNotFound,
                    $"Setting key '{key}' is not registered in group '{canonicalGroup}'.");
            }

            if (!string.Equals(definition.Group, canonicalGroup, StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(
                    SettingErrorCode.DefinitionNotFound,
                    $"Setting key '{key}' does not belong to group '{canonicalGroup}'.");
            }
        }

        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var repo = await _unitOfWork.GetRepositoryAsync<IRepository<TSetting>>(cancellationToken).ConfigureAwait(false);
        var existing = await repo.LoadAsync(x => x.Group == canonicalGroup, cancellationToken).ConfigureAwait(false);
        var existingByKey = existing.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        var results = new List<SettingModel>(values.Count);
        var touchedKeys = new List<string>(values.Count);

        foreach (var (key, value) in values)
        {
            var definition = definitionByKey[key];
            if (definition.IsReadOnly)
                throw new BadRequestException(SettingErrorCode.ReadOnly, $"Setting '{definition.Key}' is read-only and cannot be updated.");

            EnsureSecretCanPersist(definition, value);

            if (existingByKey.TryGetValue(definition.Key, out var entity))
            {
                if (entity.IsReadOnly)
                    throw new BadRequestException(SettingErrorCode.ReadOnly, $"Setting '{definition.Key}' is read-only and cannot be updated.");

                SettingValueValidator.Validate(definition, value);
                ApplyDefinitionMetadata(entity, definition);
                entity.Value = PersistValue(value, definition);
                await repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
                touchedKeys.Add(definition.Key);
                results.Add(ToModel(entity, definition));
            }
            else
            {
                SettingValueValidator.Validate(definition, value);

                // Insert mới trong cùng transaction SaveChanges phía dưới.
                entity = CreateEntity();
                entity.Id = Guid.CreateVersion7();
                entity.TenantId = tenantId;
                ApplyDefinitionMetadata(entity, definition);
                entity.Value = PersistValue(value, definition);
                await repo.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
                results.Add(ToModel(entity, definition));
                touchedKeys.Add(definition.Key);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Invalidate các Key đã ghi trong batch này.
        foreach (var touchedKey in touchedKeys)
            await DeleteCacheAsync(tenantId, touchedKey, cancellationToken).ConfigureAwait(false);

        return results
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var definition = _registry.GetSetting(key);
        var canonicalKey = definition?.Key ?? ResolveCanonicalKey(key);
        var tenantId = await RequireTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var repo = await _unitOfWork.GetRepositoryAsync<IRepository<TSetting>>(cancellationToken).ConfigureAwait(false);
        var entity = await repo.GetByIdAsync(x => x.Key == canonicalKey, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(SettingErrorCode.NotFound, $"Setting '{canonicalKey}' was not found.");

        if (entity.IsReadOnly || definition?.IsReadOnly == true)
            throw new BadRequestException(SettingErrorCode.ReadOnly, $"Setting '{canonicalKey}' is read-only and cannot be deleted.");

        await repo.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await DeleteCacheAsync(tenantId, canonicalKey, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Tạo instance entity không chạy constructor (tránh phụ thuộc <c>new()</c> / required members của host).
    /// Các property bắt buộc được gán ngay sau đó bởi Manager.
    /// </summary>
    private static TSetting CreateEntity() =>
        (TSetting)RuntimeHelpers.GetUninitializedObject(typeof(TSetting));

    /// <summary>
    /// Chuẩn hóa Key theo casing đã đăng ký trong Library (nếu có).
    /// </summary>
    private string ResolveCanonicalKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _registry.GetSetting(key)?.Key ?? key;
    }

    /// <summary>
    /// Chuẩn hóa tên Group theo casing đã đăng ký trong Library (nếu có).
    /// </summary>
    private string ResolveCanonicalGroup(string group)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        return _registry.GetGroup(group)?.Name ?? group;
    }

    /// <summary>
    /// Đọc TenantId từ <see cref="ICurrentTenant{TTenant}"/>.
    /// Thiếu tenant → <see cref="SettingErrorCode.TenantRequired"/>.
    /// </summary>
    private async Task<Guid> RequireTenantIdAsync(CancellationToken cancellationToken)
    {
        var tenantId = await _currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty)
            throw new BadRequestException(SettingErrorCode.TenantRequired, "Tenant context is required to access settings.");

        return tenantId.Value;
    }

    /// <summary>
    /// Tạo tham số cache theo Tenant + Key.
    /// Suffix <c>:persisted</c> tránh đụng entry cũ từng lưu plaintext trước khi sửa bảo mật cache.
    /// </summary>
    private CacheParam CreateCacheParam(Guid tenantId, string key) =>
        CacheParam.Create(CacheItemName)
            .WithParam("tenantId", tenantId.ToString())
            .WithParam("key", $"{key}:persisted");

    /// <summary>Xóa cache một Key sau khi create/update/delete.</summary>
    private Task DeleteCacheAsync(Guid tenantId, string key, CancellationToken cancellationToken) =>
        _cache.RemoveAsync(CreateCacheParam(tenantId, key), cancellationToken);

    /// <summary>
    /// Đồng bộ metadata snapshot từ Library xuống entity (Create và mỗi lần Update/SaveGroup).
    /// Đảm bảo Options/Type/Name… trên DB khớp definition sau khi đổi code provider.
    /// </summary>
    private static void ApplyDefinitionMetadata(TSetting entity, SettingDefinition definition)
    {
        entity.Group = definition.Group;
        entity.Key = definition.Key;
        entity.Name = definition.Name;
        entity.Type = definition.Type;
        entity.Options = definition.Options;
        entity.Description = definition.Description;
        entity.IsReadOnly = definition.IsReadOnly;
    }

    /// <summary>Xác định definition có phải secret cần mã hóa hay không.</summary>
    private static bool IsSecretDefinition(SettingDefinition? definition) =>
        definition?.IsEncrypted == true
        || string.Equals(definition?.Type, SettingValueTypes.Password, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Secret (Password / IsEncrypted) không được persist khi rỗng hoặc chỉ whitespace —
    /// không mã hóa và không ghi DB; caller nhận <see cref="SettingErrorCode.InvalidValue"/>.
    /// </summary>
    private static void EnsureSecretCanPersist(SettingDefinition definition, string plaintext)
    {
        if (!IsSecretDefinition(definition))
            return;

        if (string.IsNullOrWhiteSpace(plaintext))
        {
            throw new BadRequestException(
                SettingErrorCode.InvalidValue,
                $"Setting '{definition.Key}' is a secret and cannot be saved with an empty value.");
        }
    }

    /// <summary>
    /// Chuẩn bị Value trước khi ghi DB: secret → Encrypt; còn lại giữ plaintext.
    /// Caller phải gọi <see cref="EnsureSecretCanPersist"/> trước (secret rỗng không tới đây).
    /// </summary>
    private string PersistValue(string plaintext, SettingDefinition definition) =>
        IsSecretDefinition(definition) ? Encrypt(plaintext) : plaintext;

    /// <summary>
    /// Map entity → model đúng như lưu trữ (Value có thể là ciphertext).
    /// Dùng khi ghi vào cache.
    /// </summary>
    private static SettingModel ToPersistedModel(TSetting entity) =>
        new()
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Group = entity.Group,
            Key = entity.Key,
            Name = entity.Name,
            Value = entity.Value ?? string.Empty,
            Type = entity.Type,
            Options = entity.Options,
            Description = entity.Description,
            IsReadOnly = entity.IsReadOnly,
        };

    /// <summary>
    /// Chuyển model persisted sang plaintext cho caller (decrypt nếu secret hoặc có prefix mã hóa).
    /// </summary>
    private SettingModel ToPlaintextModel(SettingModel persisted, SettingDefinition? definition = null)
    {
        definition ??= _registry.GetSetting(persisted.Key);
        var value = persisted.Value;

        if (IsSecretDefinition(definition) || AesGcmStringEncryptionHelper.IsEncryptedPayload(value))
            value = Decrypt(value);

        return new SettingModel
        {
            Id = persisted.Id,
            TenantId = persisted.TenantId,
            Group = persisted.Group,
            Key = persisted.Key,
            Name = persisted.Name,
            Value = value,
            Type = persisted.Type,
            Options = persisted.Options,
            Description = persisted.Description,
            IsReadOnly = persisted.IsReadOnly,
        };
    }

    /// <summary>Gọi helper mã hóa; map lỗi khóa → <see cref="SettingErrorCode.EncryptionKeyInvalid"/>.</summary>
    private string Encrypt(string plaintext)
    {
        try
        {
            return AesGcmStringEncryptionHelper.Encrypt(plaintext, _encryptionOptions.DataEncryptionKey!);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(SettingErrorCode.EncryptionKeyInvalid, ex.Message, ex);
        }
    }

    /// <summary>Gọi helper giải mã; map lỗi khóa/payload → <see cref="SettingErrorCode.EncryptionKeyInvalid"/>.</summary>
    private string Decrypt(string ciphertext)
    {
        try
        {
            return AesGcmStringEncryptionHelper.Decrypt(ciphertext, _encryptionOptions.DataEncryptionKey!);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(SettingErrorCode.EncryptionKeyInvalid, ex.Message, ex);
        }
        catch (FormatException ex)
        {
            throw new BadRequestException(SettingErrorCode.EncryptionKeyInvalid, "Encrypted payload is invalid.", ex);
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            throw new BadRequestException(SettingErrorCode.EncryptionKeyInvalid, "Encrypted payload is invalid.", ex);
        }
    }

    /// <summary>Map entity → model plaintext (tiện cho các nhánh CRUD).</summary>
    private SettingModel ToModel(TSetting entity, SettingDefinition? definition = null) =>
        ToPlaintextModel(ToPersistedModel(entity), definition);
}
