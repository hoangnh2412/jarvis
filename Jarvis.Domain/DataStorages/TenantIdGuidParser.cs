namespace Jarvis.Domain.DataStorages;

internal static class TenantIdGuidParser
{
    public static Guid? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Guid.TryParse(value, out var id) ? id : null;
    }
}
