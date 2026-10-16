namespace Sample.ImportExport.Constants;

/// <summary>
/// Registry + keyed DI keys — must match <c>ConfigureEntities</c> and <c>AddImportModulePersister</c>.
/// Add new module: one const here, then use ONLY this const everywhere.
/// </summary>
public static class ImportExportEntityKeys
{
    public const string Student = "Student";
    public const string User = "User";
}
