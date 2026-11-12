namespace Platform.Permissions.Abstractions;

public sealed record PermissionDescriptor(
    string Code,
    string Module,
    string Group,
    string DisplayName,
    int ModuleOrder = int.MaxValue,
    int FieldOrder = int.MaxValue);
