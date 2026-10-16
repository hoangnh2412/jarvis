namespace Platform.BlobStoring.Synology;

public static class SynologyBlobStoringDefaults
{
    public const string ConfigurationSection = "Synology";

    public const int DefaultAutoSelectPriority = 23;

    public const int ListPageSize = 1000;

    /// <summary>SYNO.API.Auth version for DSM 7+.</summary>
    public const int AuthApiVersion = 6;

    /// <summary>Default HTTP timeout for DSM Web API calls.</summary>
    public static readonly TimeSpan HttpRequestTimeout = TimeSpan.FromMinutes(5);

    /// <summary>SYNO.API.Auth / session error codes that indicate an expired or invalid SID.</summary>
    public static bool IsSessionExpiredError(int code) => code is 105 or 106 or 119 or 401;

    public static bool IsEnabled(SynologyBlobOptions options) =>
        !string.IsNullOrWhiteSpace(options.Host)
        && !string.IsNullOrWhiteSpace(options.Username)
        && !string.IsNullOrWhiteSpace(options.Password);

    public static int ResolveAutoSelectPriority(SynologyBlobOptions options) =>
        options.AutoSelectPriority > 0 ? options.AutoSelectPriority : DefaultAutoSelectPriority;

    public static int ResolvePort(SynologyBlobOptions options) =>
        options.Port > 0 ? options.Port : options.UseHttps ? 5001 : 5000;
}
