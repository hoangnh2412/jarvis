using Platform.Modules.Setting;
using Platform.Modules.Setting.Definitions;

namespace Sample.Settings;

/// <summary>
/// Maps business modules (Student, User, …) to Excel template codes
/// configured in the ImportExport template module.
/// </summary>
public sealed class ImportExportSettingDefinition : ISettingDefinitionProvider
{
    public const string ImportGroupName = "ImportGroup";
    public const string ExportGroupName = "ExportGroup";

    /** @deprecated Use <see cref="ImportGroupName"/> — kept for backward-compatible messages. */
    public const string GroupName = ImportGroupName;

    public const string StudentImportTemplateCodeKey = "ImportExport.Student.ImportTemplateCode";
    public const string StudentExportTemplateCodeKey = "ImportExport.Student.ExportTemplateCode";

    public const string UserImportTemplateCodeKey = "ImportExport.User.ImportTemplateCode";
    public const string UserExportTemplateCodeKey = "ImportExport.User.ExportTemplateCode";

    public void Define(ISettingDefinitionContext context)
    {
        context.AddGroup(ImportGroupName, group =>
        {
            group.DisplayName = "Import";
            group.Description =
                "Gán template Excel dùng khi import dữ liệu (giá trị = template code).";
            group.Order = 5;
        });

        context.AddGroup(ExportGroupName, group =>
        {
            group.DisplayName = "Export";
            group.Description =
                "Gán template Excel dùng khi export dữ liệu (giá trị = template code).";
            group.Order = 6;
        });

        context.AddSetting(ImportGroupName, StudentImportTemplateCodeKey, setting =>
        {
            setting.Name = "Student — Import template code";
            setting.Description = "Template code dùng khi import học sinh (vd: student-import-v1).";
            setting.Type = SettingValueTypes.Text;
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(ImportGroupName, UserImportTemplateCodeKey, setting =>
        {
            setting.Name = "User — Import template code";
            setting.Description = "Template code dùng khi import người dùng.";
            setting.Type = SettingValueTypes.Text;
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(ExportGroupName, StudentExportTemplateCodeKey, setting =>
        {
            setting.Name = "Student — Export template code";
            setting.Description = "Template code dùng khi export học sinh.";
            setting.Type = SettingValueTypes.Text;
            setting.DefaultValue = string.Empty;
        });

        context.AddSetting(ExportGroupName, UserExportTemplateCodeKey, setting =>
        {
            setting.Name = "User — Export template code";
            setting.Description = "Template code dùng khi export người dùng.";
            setting.Type = SettingValueTypes.Text;
            setting.DefaultValue = string.Empty;
        });
    }
}
