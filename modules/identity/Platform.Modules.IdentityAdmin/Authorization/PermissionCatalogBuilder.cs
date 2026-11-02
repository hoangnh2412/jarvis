using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Platform.Modules.IdentityAdmin.Authorization;

/// <summary>
/// Quét các class <see cref="IPermissionDefinition"/> trong những assembly được khai báo
/// (assembly platform + assembly của host chứa mã quyền nghiệp vụ) và dựng catalog cho UI gán vai trò.
/// </summary>
public static class PermissionCatalogBuilder
{
    public static IReadOnlyList<PermissionDescriptor> Build(params Assembly[] assemblies)
    {
        var items = new List<PermissionDescriptor>();

        foreach (var assembly in assemblies.Distinct())
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !typeof(IPermissionDefinition).IsAssignableFrom(type))
                continue;

            var moduleDisplay = type.GetCustomAttribute<DisplayAttribute>();
            if (moduleDisplay is null)
                continue;

            var module = moduleDisplay.GetName();
            if (string.IsNullOrWhiteSpace(module))
                continue;

            var moduleOrder = moduleDisplay.GetOrder() ?? int.MaxValue;
            var moduleGroupFallback = moduleDisplay.GetDescription() ?? module;

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (!field.IsLiteral || field.IsInitOnly || field.FieldType != typeof(string))
                    continue;

                var fieldDisplay = field.GetCustomAttribute<DisplayAttribute>();
                if (fieldDisplay is null)
                    continue;

                var code = field.GetRawConstantValue() as string;
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                var displayName = fieldDisplay.GetDescription();
                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = fieldDisplay.GetName() ?? code;

                var group = fieldDisplay.GetGroupName();
                if (string.IsNullOrWhiteSpace(group))
                    group = moduleGroupFallback;

                var fieldOrder = fieldDisplay.GetOrder() ?? int.MaxValue;

                items.Add(new PermissionDescriptor(
                    code,
                    module,
                    group,
                    displayName,
                    moduleOrder,
                    fieldOrder));
            }
        }

        return items
            .OrderBy(item => item.ModuleOrder)
            .ThenBy(item => item.FieldOrder)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ToList();
    }
}
