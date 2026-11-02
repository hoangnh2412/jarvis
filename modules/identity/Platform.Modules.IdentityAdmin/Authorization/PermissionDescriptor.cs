namespace Platform.Modules.IdentityAdmin.Authorization;

public sealed record PermissionDescriptor(
    string Code,
    string Module,
    string Group,
    string DisplayName,
    int ModuleOrder = int.MaxValue,
    int FieldOrder = int.MaxValue);

public sealed record PolicyDescriptor(string Code, string Name, bool IsSystemPolicy);
