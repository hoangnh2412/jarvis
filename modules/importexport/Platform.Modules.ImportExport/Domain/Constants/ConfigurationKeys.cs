namespace Platform.Modules.ImportExport.Domain.Constants;

/// <summary>Configuration key names shared by API host and design-time factory.</summary>
public static class ConfigurationKeys
{
    public const string ExcelEngineConnection = "ExcelEngine";
    public const string ExcelEngineOptionsSection = "ExcelEngine";
    /// <summary>Same key as Sample / Platform.EntityFramework — enable MigrateAsync on startup.</summary>
    public const string AutoMigrate = "ConnectionStrings:AutoMigrate";
    public const string SecurityApiKey = "Security:ApiKey";
    public const string AppSettingsFile = "appsettings.json";
    public const string AppSettingsDevelopmentFile = "appsettings.Development.json";
    public const string LogsDirectory = "logs";
    public const string LogFileNamePattern = "excel-engine-.log";
    public const string ApplicationJson = "application/json";
    public const string MultipartFormData = "multipart/form-data";
}