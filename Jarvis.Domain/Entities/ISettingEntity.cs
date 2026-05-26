namespace Jarvis.Domain.Entities;

public interface ISettingEntity : ITenantEntity, IEntity<Guid>
{
    public string Key { get; set; }

    public string Value { get; set; }

    public string Type { get; set; }

    public string Options { get; set; }
}