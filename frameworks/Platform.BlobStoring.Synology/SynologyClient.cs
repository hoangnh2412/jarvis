using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Platform.BlobStoring;
using Platform.BlobStoring.Helpers;

namespace Platform.BlobStoring.Synology;

/// <summary>
/// Thin DSM Drive HTTP client. Public surface: session, CRUD, list, folder, sharing.
/// Infrastructure: <see cref="SendAsync{T}"/>, <see cref="BuildApiUri"/>, multipart helpers.
/// </summary>
internal sealed class SynologyClient : IDisposable
{
    private const string DriveSession = "SynologyDrive";
    private const string EntryCgi = "entry.cgi";
    private const string AuthCgi = "auth.cgi";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SynologyBlobOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _authLock = new(1, 1);
    private string? _sessionId;
    private bool _disposed;

    public SynologyClient(SynologyBlobOptions options, ILogger logger)
    {
        _options = options;
        _logger = logger;
        _httpClient = new HttpClient(new HttpClientHandler())
        {
            Timeout = SynologyBlobStoringDefaults.HttpRequestTimeout
        };
    }

    public async Task<string> GetSessionIdAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_sessionId))
            return _sessionId;

        await _authLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrEmpty(_sessionId))
                return _sessionId;

            _logger.LogDebug("Logging in to Synology DSM as {Username}", _options.Username);

            var uri = BuildApiUri(AuthCgi, new Dictionary<string, string?>
            {
                ["api"] = "SYNO.API.Auth",
                ["version"] = SynologyBlobStoringDefaults.AuthApiVersion.ToString(),
                ["method"] = "login",
                ["account"] = _options.Username.Trim(),
                ["passwd"] = _options.Password,
                ["session"] = DriveSession,
                ["format"] = "cookie"
            });

            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var envelope = await response.Content
                .ReadFromJsonAsync<SynologyApiEnvelope<SynologyAuthData>>(JsonOptions, cancellationToken)
                ?? throw new InvalidOperationException("Synology login response was empty.");

            if (!envelope.Success || string.IsNullOrWhiteSpace(envelope.Data?.Sid))
            {
                var code = envelope.Error?.Code ?? -1;
                throw new InvalidOperationException(
                    $"Synology login failed with error code {code}. {DescribeAuthError(code)}");
            }

            _sessionId = envelope.Data.Sid;
            return _sessionId;
        }
        finally
        {
            _authLock.Release();
        }
    }

    public string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var normalized = path.Replace('\\', '/').Trim('/');
        if (normalized.Equals("MyDrive", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("MyDrive/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = "mydrive" + normalized["MyDrive".Length..];
        }

        return normalized;
    }

    public async Task<SynologyDriveItem?> GetItemAsync(
        string drivePath,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithSessionRetryAsync(async (sid, ct) =>
        {
            var uri = BuildApiUri(EntryCgi, new Dictionary<string, string?> { ["_sid"] = sid });
            using var content = BuildFormBody(new Dictionary<string, object>
            {
                ["api"] = "SYNO.SynologyDrive.Files",
                ["method"] = "get",
                ["version"] = 3,
                ["path"] = ToApiPath(drivePath)
            });

            var envelope = await SendAsync<SynologyDriveItem>(HttpMethod.Post, uri, content, ct);
            return envelope.Success ? envelope.Data : null;
        }, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string drivePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(NormalizePath(drivePath)))
            return true;

        return await GetItemAsync(drivePath, cancellationToken) is not null;
    }

    public async Task UploadAsync(
        string drivePath,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        await using var stream = new MemoryStream(content);
        await UploadStreamAsync(drivePath, stream, content.Length, cancellationToken);
    }

    public async Task UploadStreamAsync(
        string drivePath,
        Stream content,
        long? contentLength = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        await ExecuteWithSessionRetryAsync(async (sid, ct) =>
        {
            var apiPath = ToApiPath(drivePath);
            var fileName = GetFileName(apiPath);

            _logger.LogDebug(
                "Uploading stream to Synology path {Path}, contentLength={ContentLength}",
                apiPath,
                contentLength?.ToString() ?? "(unknown)");

            var boundary = "----PlatformSynologyBoundary" + Guid.NewGuid().ToString("N");
            using var streamContent = new StreamContent(content);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

            var size = BlobStoringStreamHelper.ResolveUploadSize(content, contentLength);
            if (size is > 0)
                streamContent.Headers.ContentLength = size;

            using var httpContent = new MultipartFormDataContent(boundary);
            httpContent.Add(streamContent, "file", fileName);

            var uri = BuildApiUri(EntryCgi, new Dictionary<string, string?>
            {
                ["api"] = "SYNO.SynologyDrive.Files",
                ["method"] = "upload",
                ["version"] = "2",
                ["path"] = apiPath,
                ["type"] = "file",
                ["conflict_action"] = "version",
                ["_sid"] = sid
            });

            await SendAsync<JsonElement>(HttpMethod.Post, uri, httpContent, ct, requireSuccess: true, "Files.upload");
            return true;
        }, cancellationToken);
    }

    public async Task<byte[]> DownloadAsync(
        string drivePath,
        CancellationToken cancellationToken = default)
    {
        await using var result = await OpenReadAsync(drivePath, cancellationToken);
        return await BlobStoringStreamHelper.ReadAllBytesAsync(result.Content, cancellationToken);
    }

    public async Task<BlobReadResult> OpenReadAsync(
        string drivePath,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithSessionRetryAsync(async (sid, ct) =>
        {
            var item = await GetItemAsync(drivePath, ct)
                ?? throw new InvalidOperationException($"Synology Drive item not found at path '{drivePath}'.");
            if (item.FileId is null)
                throw new InvalidOperationException($"Synology Drive item not found at path '{drivePath}'.");

            var fileName = item.Name ?? GetFileName(NormalizePath(drivePath));
            var filesParam = JsonSerializer.Serialize(new[] { $"id:{item.FileId}" });
            var uri = BuildApiUri($"{EntryCgi}/{Uri.EscapeDataString(fileName)}", new Dictionary<string, string?>
            {
                ["api"] = "SYNO.SynologyDrive.Files",
                ["method"] = "download",
                ["version"] = "2",
                ["files"] = filesParam,
                ["force_download"] = "true",
                ["json_error"] = "true",
                ["_sid"] = sid
            });

            var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(ct);
            return BlobReadResult.FromStream(
                stream,
                contentType: response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
                contentLength: response.Content.Headers.ContentLength,
                disposeAsync: () =>
                {
                    response.Dispose();
                    return ValueTask.CompletedTask;
                });
        }, cancellationToken);
    }

    public async Task DeleteAsync(
        string drivePath,
        CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(drivePath, cancellationToken);
        if (item?.FileId is null)
        {
            _logger.LogDebug("Synology item already missing at path {Path}", drivePath);
            return;
        }

        await ExecuteWithSessionRetryAsync(async (sid, ct) =>
        {
            var uri = BuildApiUri(EntryCgi, new Dictionary<string, string?> { ["_sid"] = sid });
            using var content = BuildFormBody(new Dictionary<string, object>
            {
                ["api"] = "SYNO.SynologyDrive.Files",
                ["method"] = "delete",
                ["version"] = 2,
                ["files"] = new[] { $"id:{item.FileId}" },
                ["permanent"] = "false",
                ["revisions"] = ResolveRevisionsPayload(item)
            });

            await SendAsync<JsonElement>(
                HttpMethod.Post,
                uri,
                content,
                ct,
                requireSuccess: true,
                $"Files.delete ({drivePath})");

            _logger.LogDebug("Deleted Synology item at {Path}, fileId={FileId}", drivePath, item.FileId);
            return true;
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<SynologyDriveItem>> ListFolderAsync(
        string drivePath,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithSessionRetryAsync(async (sid, ct) =>
        {
            var items = new List<SynologyDriveItem>();
            var offset = 0;
            var apiPath = ToApiPath(drivePath);

            while (true)
            {
                var uri = BuildApiUri(EntryCgi, new Dictionary<string, string?>
                {
                    ["api"] = "SYNO.SynologyDrive.Files",
                    ["method"] = "list",
                    ["version"] = "2",
                    ["sort_direction"] = "asc",
                    ["sort_by"] = "owner",
                    ["offset"] = offset.ToString(),
                    ["limit"] = SynologyBlobStoringDefaults.ListPageSize.ToString(),
                    ["path"] = apiPath,
                    ["_sid"] = sid
                });

                var envelope = await SendAsync<SynologyDriveListData>(HttpMethod.Get, uri, content: null, ct);
                if (!envelope.Success)
                    throw new InvalidOperationException($"Synology list failed with error code {envelope.Error?.Code ?? -1}.");

                var page = envelope.Data ?? new SynologyDriveListData();
                items.AddRange(page.Items);

                offset += page.Items.Count;
                if (offset >= page.Total || page.Items.Count == 0)
                    break;
            }

            return (IReadOnlyList<SynologyDriveItem>)items;
        }, cancellationToken);
    }

    public async Task CreateFolderAsync(
        string drivePath,
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithSessionRetryAsync(async (sid, ct) =>
        {
            var uri = BuildApiUri(EntryCgi, new Dictionary<string, string?>
            {
                ["api"] = "SYNO.SynologyDrive.Files",
                ["method"] = "create",
                ["version"] = "2",
                ["path"] = ToApiPath(drivePath),
                ["type"] = "folder",
                ["conflict_action"] = "autorename",
                ["_sid"] = sid
            });

            await SendAsync<JsonElement>(HttpMethod.Put, uri, content: null, ct, requireSuccess: true, "Files.create");
            return true;
        }, cancellationToken);
    }

    public async Task RenameAsync(
        string drivePath,
        string newName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        await ExecuteWithSessionRetryAsync(async (sid, ct) =>
        {
            var uri = BuildApiUri(EntryCgi, new Dictionary<string, string?> { ["_sid"] = sid });
            using var content = BuildFormBody(new Dictionary<string, object>
            {
                ["api"] = "SYNO.SynologyDrive.Files",
                ["method"] = "update",
                ["version"] = "2",
                ["path"] = ToApiPath(drivePath),
                ["name"] = newName
            });

            await SendAsync<JsonElement>(HttpMethod.Post, uri, content, ct, requireSuccess: true, "Files.update");
            return true;
        }, cancellationToken);
    }

    public async Task MoveAsync(
        string drivePath,
        string destinationParentPath,
        CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(drivePath, cancellationToken)
            ?? throw new InvalidOperationException($"Synology Drive item not found at path '{drivePath}'.");

        if (item.FileId is null)
            throw new InvalidOperationException($"Synology Drive item not found at path '{drivePath}'.");

        await ExecuteWithSessionRetryAsync(async (sid, ct) =>
        {
            var uri = BuildApiUri(EntryCgi, new Dictionary<string, string?> { ["_sid"] = sid });
            using var content = BuildFormBody(new Dictionary<string, object>
            {
                ["api"] = "SYNO.SynologyDrive.Files",
                ["method"] = "move",
                ["version"] = "2",
                ["files"] = new[] { $"id:{item.FileId}" },
                ["to_parent_folder"] = ToApiPath(destinationParentPath),
                ["conflict_action"] = "autorename"
            });

            await SendAsync<JsonElement>(HttpMethod.Post, uri, content, ct, requireSuccess: true, "Files.move");
            return true;
        }, cancellationToken);
    }

    public async Task<string?> CreateSharingLinkAsync(
        string drivePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ExecuteWithSessionRetryAsync(async (sid, ct) =>
            {
                var uri = BuildApiUri(EntryCgi, new Dictionary<string, string?> { ["_sid"] = sid });
                using var content = BuildFormBody(new Dictionary<string, object>
                {
                    ["api"] = "SYNO.SynologyDrive.Sharing",
                    ["method"] = "create_link",
                    ["version"] = 1,
                    ["path"] = ToApiPath(drivePath)
                });

                var envelope = await SendAsync<SynologySharingLinkData>(HttpMethod.Post, uri, content, ct);
                if (!envelope.Success)
                    return null;

                return envelope.Data?.Url ?? envelope.Data?.Link;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Synology sharing link failed for {Path}", drivePath);
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _httpClient.Dispose();
        _authLock.Dispose();
        _disposed = true;
    }

    /// <summary>Build absolute DSM Web API URI: <c>{scheme}://{host}:{port}/webapi/{script}?...</c>.</summary>
    public Uri BuildApiUri(string script, IReadOnlyDictionary<string, string?>? query = null)
    {
        var authority = BuildAuthority();
        var path = script.StartsWith('/') ? script : $"/webapi/{script}";
        var builder = new UriBuilder($"{authority}{path}");

        if (query is { Count: > 0 })
        {
            builder.Query = string.Join(
                "&",
                query.Where(static p => !string.IsNullOrEmpty(p.Value))
                    .Select(static p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}"));
        }

        return builder.Uri;
    }

    private async Task<SynologyApiEnvelope<T>> SendAsync<T>(
        HttpMethod method,
        Uri uri,
        HttpContent? content,
        CancellationToken cancellationToken,
        bool requireSuccess = false,
        string? operation = null)
    {
        using var request = new HttpRequestMessage(method, uri) { Content = content };
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var envelope = await JsonSerializer.DeserializeAsync<SynologyApiEnvelope<T>>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Synology API response was empty.");

        if (!envelope.Success && SynologyBlobStoringDefaults.IsSessionExpiredError(envelope.Error?.Code ?? -1))
            throw new SynologySessionExpiredException(envelope.Error!.Code);

        if (requireSuccess && !envelope.Success)
        {
            var code = envelope.Error?.Code ?? -1;
            var detail = ExtractErrorDetail(envelope);
            var op = operation ?? $"{method} {uri.AbsolutePath}";
            throw new InvalidOperationException(
                string.IsNullOrEmpty(detail)
                    ? $"{op} failed with error code {code}."
                    : $"{op} failed with error code {code}: {detail}");
        }

        return envelope;
    }

    private async Task<T> ExecuteWithSessionRetryAsync<T>(
        Func<string, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                var sid = await GetSessionIdAsync(cancellationToken);
                return await action(sid, cancellationToken);
            }
            catch (SynologySessionExpiredException) when (attempt == 0)
            {
                _sessionId = null;
                _logger.LogDebug("Synology session expired; re-authenticating.");
            }
        }

        throw new InvalidOperationException("Synology request failed after session retry.");
    }

    private sealed class SynologySessionExpiredException(int code)
        : InvalidOperationException($"Synology session expired (error code {code}).");

    private string BuildAuthority()
    {
        var host = _options.Host.Trim();
        var port = SynologyBlobStoringDefaults.ResolvePort(_options);
        var useHttps = _options.UseHttps || port == 443;
        var scheme = useHttps ? "https" : "http";
        var isDefaultPort = useHttps ? port == 443 : port == 80;
        return isDefaultPort ? $"{scheme}://{host}" : $"{scheme}://{host}:{port}";
    }

    private string ToApiPath(string drivePath)
    {
        var normalized = NormalizePath(drivePath);
        return string.IsNullOrEmpty(normalized) ? "/" : $"/{normalized}";
    }

    private static string GetFileName(string apiPath)
    {
        var trimmed = apiPath.Trim('/');
        var slash = trimmed.LastIndexOf('/');
        return slash < 0 ? trimmed : trimmed[(slash + 1)..];
    }

    private static byte[] BuildUploadMultipartBody(string boundary, string fileName, byte[] content)
    {
        var header =
            $"--{boundary}\r\n" +
            $"Content-Disposition: form-data; name=\"file\"; filename=\"{EscapeMultipartFileName(fileName)}\"\r\n" +
            "Content-Type: application/octet-stream\r\n\r\n";
        var headerBytes = Encoding.UTF8.GetBytes(header);
        var footerBytes = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");

        var body = new byte[headerBytes.Length + content.Length + footerBytes.Length];
        Buffer.BlockCopy(headerBytes, 0, body, 0, headerBytes.Length);
        Buffer.BlockCopy(content, 0, body, headerBytes.Length, content.Length);
        Buffer.BlockCopy(footerBytes, 0, body, headerBytes.Length + content.Length, footerBytes.Length);
        return body;
    }

    private static string EscapeMultipartFileName(string fileName) =>
        fileName.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    private static StringContent BuildFormBody(Dictionary<string, object> data)
    {
        var parts = data.Select(static pair =>
        {
            var encodedValue = pair.Value switch
            {
                string text => Uri.EscapeDataString(text),
                _ => Uri.EscapeDataString(JsonSerializer.Serialize(pair.Value))
            };
            return $"{Uri.EscapeDataString(pair.Key)}={encodedValue}";
        });

        return new StringContent(string.Join("&", parts), Encoding.UTF8, "application/x-www-form-urlencoded");
    }

    private static object ResolveRevisionsPayload(SynologyDriveItem item)
    {
        if (item.Revisions is not { } revisions)
            return Array.Empty<object>();

        return JsonSerializer.Deserialize<object>(revisions.GetRawText()) ?? Array.Empty<object>();
    }

    private static string? ExtractErrorDetail<T>(SynologyApiEnvelope<T> envelope)
    {
        if (envelope.Error?.Errors is not { } errors)
            return null;

        if (errors.ValueKind == JsonValueKind.Object
            && errors.TryGetProperty("message", out var message)
            && message.ValueKind == JsonValueKind.String)
        {
            return message.GetString();
        }

        return errors.ToString();
    }

    private static string DescribeAuthError(int code) => code switch
    {
        400 => "Sai username hoặc password.",
        401 => "Tài khoản bị khóa / vô hiệu hoặc credential không còn hợp lệ.",
        403 or 404 => "Tài khoản bật 2FA — dùng service account không OTP (tắt 2FA hoặc app password).",
        _ => "Kiểm tra Username/Password (Web và API phải cùng credential)."
    };

    private sealed class SynologyApiEnvelope<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("data")]
        public T? Data { get; init; }

        [JsonPropertyName("error")]
        public SynologyApiError? Error { get; init; }
    }

    private sealed class SynologyApiError
    {
        [JsonPropertyName("code")]
        public int Code { get; init; }

        [JsonPropertyName("errors")]
        public JsonElement? Errors { get; init; }
    }

    private sealed class SynologyAuthData
    {
        [JsonPropertyName("sid")]
        public string? Sid { get; init; }
    }

    private sealed class SynologyDriveListData
    {
        [JsonPropertyName("items")]
        public List<SynologyDriveItem> Items { get; init; } = [];

        [JsonPropertyName("total")]
        public int Total { get; init; }
    }

    private sealed class SynologySharingLinkData
    {
        [JsonPropertyName("url")]
        public string? Url { get; init; }

        [JsonPropertyName("link")]
        public string? Link { get; init; }
    }
}

internal sealed class SynologyDriveItem
{
    [JsonPropertyName("file_id")]
    public string? FileId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("path")]
    public string? Path { get; init; }

    [JsonPropertyName("display_path")]
    public string? DisplayPath { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("revisions")]
    public JsonElement? Revisions { get; init; }

    public bool IsFolder =>
        string.Equals(Type, "folder", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Type, "dir", StringComparison.OrdinalIgnoreCase);
}

