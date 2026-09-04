namespace Jarvis.DDD.Domain.Entities;

/// <summary>
/// Tenant-scoped setting stored as one row per key.
/// Group metadata (display name, order, etc.) lives in code/registry, not in the database.
/// </summary>
public interface ISettingEntity : ITenantEntity, IEntity<Guid>
{
    /// <summary>
    /// Logical group used to bundle related settings (e.g. Email).
    /// </summary>
    string Group { get; set; }

    /// <summary>
    /// Unique setting key within a tenant.
    /// </summary>
    string Key { get; set; }

    /// <summary>
    /// Display name for consumers.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Setting value. Sensitive values should be encrypted at rest.
    /// </summary>
    string Value { get; set; }

    /// <summary>
    /// Value type (e.g. Text, Textarea, Combobox, Password).
    /// </summary>
    string Type { get; set; }

    /// <summary>
    /// Optional selectable values, e.g. yes|no or 1:Yes|0:No.
    /// </summary>
    string? Options { get; set; }

    /// <summary>
    /// Optional description.
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    /// When true, the value cannot be updated or deleted.
    /// </summary>
    bool IsReadOnly { get; set; }
}
