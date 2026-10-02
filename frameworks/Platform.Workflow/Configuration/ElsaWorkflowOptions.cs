using Microsoft.Extensions.Configuration;

namespace Platform.Workflow.Configuration;

public enum WorkflowExecutionMode
{
    Embedded = 0,
    Standalone = 1
}

public sealed class ElsaWorkflowOptions
{
    public const string SectionName = "Elsa";

    public WorkflowExecutionMode Mode { get; init; } = WorkflowExecutionMode.Embedded;

    public string ConnectionStringName { get; init; } = "MasterDbContext";

    /// <summary>
    /// Schema lưu trữ của Elsa (mặc định là "elsa").
    /// Nếu database đã có schema sẵn (ví dụ: "public" hoặc schema đã được tạo từ trước),
    /// cấu hình tên schema đó để sử dụng trực tiếp mà không tạo schema mới.
    /// </summary>
    public string PersistenceSchema { get; init; } = "elsa";

    /// <summary>
    /// Đường dẫn tương đối hoặc tiền tố route của Elsa API (mặc định là "/elsa/api").
    /// Dùng cho cả Embedded API endpoint mapping và ghép với ServerUrl trong Standalone client.
    /// Ví dụ: "/elsa/api"
    /// </summary>
    public string ApiPathPrefix { get; init; } = "/elsa/api";

    /// <summary>
    /// URL gốc của Elsa Workflow Server (chỉ chứa scheme + host + port, không bao gồm ApiPathPrefix).
    /// - Mode Standalone: Client gọi HTTP API tới server này kết hợp với ApiPathPrefix.
    /// - Mode Embedded (Studio): Blazor Studio kết nối API backend nếu chạy tách biệt.
    /// Ví dụ: "https://workflow-server:7000"
    /// </summary>
    public string? ServerUrl { get; init; }

    /// <summary>
    /// URL gốc của Elsa Studio dùng cho route preview khi ứng dụng client chạy Standalone.
    /// Nếu không cấu hình, sẽ dùng ServerUrl.
    /// </summary>
    public string? StudioUrl { get; init; }

    /// <summary>
    /// URL đầy đủ trực tiếp đến Elsa API endpoint (bao gồm cả scheme + host + port + path).
    /// Thường dùng để override tùy biến toàn bộ URL khi không tuân theo quy chuẩn ServerUrl + ApiPathPrefix.
    /// Ví dụ: "https://workflow-server:7000/custom-path/elsa/api"
    /// </summary>
    public string? ApiUrl { get; init; }

    /// <summary>
    /// API Key dùng để xác thực khi kết nối tới Elsa Server từ xa (Mode = Standalone).
    /// Nếu null, sẽ không thêm Authorization header.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// Lấy URL API hoàn chỉnh dựa trên cấu hình:
    /// 1. Ưu tiên ApiUrl nếu được chỉ định trực tiếp.
    /// 2. Ghép ServerUrl + ApiPathPrefix nếu ServerUrl có giá trị.
    /// 3. Trả về null nếu cả hai đều chưa được cấu hình.
    /// </summary>
    public string? GetResolvedApiUrl()
    {
        if (!string.IsNullOrWhiteSpace(ApiUrl))
        {
            return ApiUrl.TrimEnd('/');
        }

        if (!string.IsNullOrWhiteSpace(ServerUrl))
        {
            var server = ServerUrl.Trim().TrimEnd('/');
            var prefix = ApiPathPrefix.Trim().Trim('/');
            return string.IsNullOrEmpty(prefix) ? server : $"{server}/{prefix}";
        }

        return null;
    }

    public string? GetResolvedStudioUrl()
    {
        return string.IsNullOrWhiteSpace(StudioUrl)
            ? ServerUrl?.Trim().TrimEnd('/')
            : StudioUrl.Trim().TrimEnd('/');
    }

    /// <summary>
    /// Danh sách tiền tố đường dẫn UI của Elsa Studio / Preview cần được bảo vệ bằng authentication.
    /// Platform middleware sẽ intercept các route này và kiểm tra authorization.
    /// Mặc định: ["/workflows", "/workflow-definitions", "/workflow-instances", "/studio", "/preview"]
    /// </summary>
    public string[] StudioPathPrefixes { get; init; } = ["/workflows", "/workflow-definitions", "/workflow-instances", "/studio", "/preview"];

    /// <summary>
    /// Policy authorization dùng chung cho Workflow API và Studio.
    /// Nếu null, module không tự áp đặt policy và sử dụng authorization pipeline của host.
    /// </summary>
    public string? AuthorizationPolicy { get; init; }

    /// <summary>
    /// Policy riêng cho Studio. Nếu null sẽ dùng AuthorizationPolicy.
    /// </summary>
    public string? StudioAuthorizationPolicy { get; init; }

    /// <summary>
    /// Policy riêng cho Workflow API. Nếu null sẽ dùng AuthorizationPolicy.
    /// </summary>
    public string? ApiAuthorizationPolicy { get; init; }

    public static ElsaWorkflowOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(SectionName);

        var configuredPrefixes = section.GetSection(nameof(StudioPathPrefixes)).Get<string[]>();
        var prefixes = configuredPrefixes?
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p.StartsWith('/') ? p : "/" + p)
            .ToArray();

        if (prefixes == null || prefixes.Length == 0)
        {
            prefixes = ["/workflows", "/workflow-definitions", "/workflow-instances", "/studio", "/preview"];
        }

        var modeStr = section.GetValue<string>(nameof(Mode));
        var mode = Enum.TryParse<WorkflowExecutionMode>(modeStr, true, out var parsedMode)
            ? parsedMode
            : WorkflowExecutionMode.Embedded;

        var rawApiPathPrefix = section.GetValue<string>(nameof(ApiPathPrefix))?.Trim();
        var apiPathPrefix = string.IsNullOrEmpty(rawApiPathPrefix)
            ? "/elsa/api"
            : (rawApiPathPrefix.StartsWith('/') ? rawApiPathPrefix : "/" + rawApiPathPrefix);

        static string? NormalizeOptionalString(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        return new ElsaWorkflowOptions
        {
            Mode = mode,
            ConnectionStringName = NormalizeOptionalString(section.GetValue<string>(nameof(ConnectionStringName))) ?? "MasterDbContext",
            PersistenceSchema = NormalizeOptionalString(section.GetValue<string>(nameof(PersistenceSchema))) ?? "elsa",
            ApiPathPrefix = apiPathPrefix,
            ApiUrl = NormalizeOptionalString(section.GetValue<string>(nameof(ApiUrl))),
            ServerUrl = NormalizeOptionalString(section.GetValue<string>(nameof(ServerUrl))),
            StudioUrl = NormalizeOptionalString(section.GetValue<string>(nameof(StudioUrl))),
            ApiKey = NormalizeOptionalString(section.GetValue<string>(nameof(ApiKey))),
            StudioPathPrefixes = prefixes,
            AuthorizationPolicy = NormalizeOptionalString(section.GetValue<string>(nameof(AuthorizationPolicy))),
            StudioAuthorizationPolicy = NormalizeOptionalString(section.GetValue<string>(nameof(StudioAuthorizationPolicy))),
            ApiAuthorizationPolicy = NormalizeOptionalString(section.GetValue<string>(nameof(ApiAuthorizationPolicy))),
        };
    }
}
