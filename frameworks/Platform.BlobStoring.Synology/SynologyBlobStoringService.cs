// Platform.BlobStoring.Synology — Synology Drive provider (MinIO-style: one service + Lazy client).
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.BlobStoring.Helpers;

namespace Platform.BlobStoring.Synology;

/// <summary>
/// Lưu blob trên Synology Drive qua DSM Web API.
/// Đồng thời hiện thực <see cref="IBlobStoringService"/> và <see cref="ISynologyFolderService"/>.
/// </summary>
/// <remarks>
/// <para>
/// Pattern giống MinIO: ctor nhận Options/Logger, Lazy HTTP client, <see cref="IDisposable"/>.
/// Ánh xạ tham số contract chung sang path Drive:
/// </para>
/// <list type="bullet">
/// <item>
/// <term>RootPath</term>
/// <description>
/// Gốc lưu trữ do host cấu hình trong <see cref="SynologyBlobOptions.RootPath"/>
/// (vd. <c>/team-folders/ERP</c> hoặc <c>/mydrive</c> — path API, không phải nhãn UI).
/// </description>
/// </item>
/// <item>
/// <term>bucket</term>
/// <description>
/// Thư mục dưới <see cref="SynologyBlobOptions.RootPath"/> (vd. <c>invoices</c> hoặc <c>invoices/2024</c>).
/// Rỗng = ngay dưới RootPath.
/// </description>
/// </item>
/// <item>
/// <term>fileName</term>
/// <description>
/// Object key tương đối trong bucket (vd. <c>a.pdf</c> hoặc <c>2024/a.pdf</c>).
/// Path Drive đầy đủ: <c>{RootPath}/{bucket}/{fileName}</c>.
/// </description>
/// </item>
/// </list>
/// </remarks>
public class SynologyBlobStoringService : IBlobStoringService, ISynologyFolderService, IDisposable
{
    private readonly SynologyBlobOptions _options;
    private readonly Lazy<SynologyClient> _client;
    private readonly ILogger<SynologyBlobStoringService> _logger;
    private bool _disposed;

    /// <summary>Khởi tạo service từ Options và logger do DI cung cấp.</summary>
    public SynologyBlobStoringService(
        IOptions<SynologyBlobOptions> options,
        ILogger<SynologyBlobStoringService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new Lazy<SynologyClient>(() => new SynologyClient(_options, logger));

        if (string.IsNullOrWhiteSpace(_options.RootPath))
        {
            _logger.LogWarning(
                "Synology RootPath is empty; blob operations will target the Synology Drive API root path.");
        }
    }

    /// <summary>DSM HTTP client dùng chung; tạo lần đầu khi cần.</summary>
    private SynologyClient Client => _client.Value;

    /// <summary>
    /// Gốc Drive đã chuẩn hóa từ <see cref="SynologyBlobOptions.RootPath"/>.
    /// </summary>
    /// <example>
    /// <c>RootPath = "/team-folders/ERP"</c> → <c>team-folders/ERP</c><br/>
    /// <c>RootPath = "My Drive"</c> → <c>mydrive</c><br/>
    /// <c>RootPath = "MyDrive/docs"</c> → <c>mydrive/docs</c>
    /// </example>
    private string? RootPath
    {
        get
        {
            var root = Normalize(_options.RootPath);
            if (string.IsNullOrEmpty(root))
                return null;

            // Nhãn UI → path API (đồng bộ nhẹ với SynologyClient.NormalizePath).
            if (root.Equals("MyDrive", StringComparison.OrdinalIgnoreCase)
                || root.Equals("My Drive", StringComparison.OrdinalIgnoreCase))
                return "mydrive";

            if (root.StartsWith("MyDrive/", StringComparison.OrdinalIgnoreCase))
                return "mydrive" + root["MyDrive".Length..];

            if (root.StartsWith("My Drive/", StringComparison.OrdinalIgnoreCase))
                return "mydrive" + root["My Drive".Length..];

            return root;
        }
    }

    /// <summary>
    /// Upload file lên Synology Drive dưới <see cref="SynologyBlobOptions.RootPath"/>.
    /// Ghép <paramref name="bucket"/>/<paramref name="fileName"/> thành path Drive;
    /// ensure folder cha nếu thiếu; upload multipart qua DSM API.
    /// </summary>
    /// <param name="bucket">
    /// Folder dưới RootPath (tham số <c>bucket</c> của <see cref="IBlobStoringService"/>).
    /// Rỗng = ngay dưới RootPath. Ví dụ <c>invoices</c>.
    /// </param>
    /// <param name="fileName">Path file tương đối trong bucket đó (vd. <c>2024/a.pdf</c>).</param>
    /// <param name="bytes">Nội dung file.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <example>
    /// <c>UploadAsync("invoices", "2024/a.pdf", bytes)</c>
    /// với RootPath=<c>/team-folders/ERP</c>
    /// → ghi <c>team-folders/ERP/invoices/2024/a.pdf</c>.
    /// </example>
    public virtual async Task UploadAsync(
        string bucket,
        string fileName,
        byte[] bytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        await using var stream = new MemoryStream(bytes);
        await UploadStreamAsync(bucket, fileName, stream, bytes.Length, cancellationToken);
    }

    public virtual async Task UploadStreamAsync(
        string bucket,
        string fileName,
        Stream content,
        long? contentLength = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var path = ToDrivePath(bucket, fileName);
        await EnsureFolderAsync(GetParentFolderPath(path), cancellationToken);
        _logger.LogDebug(
            "Uploading stream for {FileName} to Synology path {Path}, contentLength={ContentLength}",
            fileName,
            path,
            contentLength?.ToString() ?? "(unknown)");

        await Client.UploadStreamAsync(path, content, contentLength, cancellationToken);
    }

    public virtual async Task<byte[]> DownloadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        await using var result = await OpenReadAsync(bucket, fileName, cancellationToken);
        return await BlobStoringStreamHelper.ReadAllBytesAsync(result.Content, cancellationToken);
    }

    public virtual Task<BlobReadResult> OpenReadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var path = ToDrivePath(bucket, fileName);
        _logger.LogDebug("Opening read stream for {FileName} from Synology path {Path}", fileName, path);
        return Client.OpenReadAsync(path, cancellationToken);
    }

    /// <summary>
    /// Tạo folder bucket rỗng dưới RootPath. Idempotent khi folder đã tồn tại.
    /// Khi <paramref name="path"/> có giá trị, tạo folder phân cấp dưới <paramref name="bucket"/>.
    /// </summary>
    /// <param name="bucket">
    /// Folder dưới RootPath. Rỗng = ngay dưới RootPath.
    /// </param>
    /// <param name="path">Folder con tương đối trong <paramref name="bucket"/> (vd. <c>2024/Q1</c>).</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task CreateBucketAsync(
        string bucket,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedBucket = Normalize(bucket);
        if (!string.IsNullOrEmpty(normalizedBucket))
            EnsureNoTraversal(normalizedBucket);

        var relativePath = string.IsNullOrWhiteSpace(path)
            ? null
            : Normalize(path);

        if (!string.IsNullOrEmpty(relativePath))
            EnsureNoTraversal(relativePath);

        var relativeFolder = string.IsNullOrEmpty(relativePath)
            ? normalizedBucket
            : CombineDrivePath(normalizedBucket, relativePath);

        var folderPath = ToDriveFolderPath(relativeFolder);
        _logger.LogDebug("Creating Synology folder bucket {Path}", folderPath);
        await EnsureFolderAsync(folderPath, cancellationToken);
    }

    /// <summary>
    /// Xóa một file (hoặc item) theo path trên Synology Drive.
    /// Lỗi DSM được trả về caller.
    /// </summary>
    /// <param name="bucket">
    /// Folder dưới RootPath. Rỗng = ngay dưới RootPath.
    /// </param>
    /// <param name="fileName">Path item tương đối trong bucket đó.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <example>
    /// <c>DeleteFileAsync("invoices", "2024/a.pdf")</c>
    /// → xóa <c>{RootPath}/invoices/2024/a.pdf</c>.
    /// </example>
    public virtual async Task DeleteFileAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var path = ToDrivePath(bucket, fileName);
        _logger.LogDebug("Deleting {FileName} from Synology path {Path}", fileName, path);
        await Client.DeleteAsync(path, cancellationToken);
    }

    /// <summary>
    /// Xóa tuần tự nhiều file trong cùng <paramref name="bucket"/>.
    /// Không lọc tên; host chịu trách nhiệm input.
    /// </summary>
    /// <param name="bucket">
    /// Folder dưới RootPath. Rỗng = ngay dưới RootPath.
    /// </param>
    /// <param name="fileNames">Danh sách path file tương đối trong bucket đó.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <example>
    /// <c>DeleteFilesAsync("invoices", ["a.pdf", "2024/b.pdf"])</c>
    /// → xóa lần lượt hai path dưới <c>{RootPath}/invoices</c>.
    /// </example>
    public virtual async Task DeleteFilesAsync(
        string bucket,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken = default)
    {
        foreach (var name in fileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DeleteFileAsync(bucket, name, cancellationToken);
        }
    }

    /// <summary>
    /// Xóa folder bucket và toàn bộ nội dung bên trong. Folder không tồn tại → no-op.
    /// Không cho phép xóa drive root.
    /// </summary>
    /// <param name="bucket">
    /// Folder dưới RootPath.
    /// </param>
    /// <param name="prefix">Subtree tương đối trong bucket; null/empty = xóa toàn bộ bucket.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task DeleteBucketAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedBucket = Normalize(bucket);
        if (!string.IsNullOrEmpty(normalizedBucket))
            EnsureNoTraversal(normalizedBucket);

        var normalizedPrefix = Normalize(prefix);
        if (!string.IsNullOrEmpty(normalizedPrefix))
            EnsureNoTraversal(normalizedPrefix);

        var relativePath = string.IsNullOrEmpty(normalizedPrefix)
            ? normalizedBucket
            : CombineDrivePath(normalizedBucket, normalizedPrefix!);

        if (string.IsNullOrEmpty(relativePath))
            throw new InvalidOperationException("Cannot delete the Synology drive root.");

        var drivePath = ToDriveFolderPath(relativePath);
        _logger.LogDebug("Deleting Synology folder bucket {Path}", drivePath);
        await Client.DeleteAsync(drivePath, cancellationToken);
    }

    /// <summary>
    /// Tạo URL xem file trên Synology Drive: ưu tiên sharing link;
    /// nếu không có thì fallback URL Drive viewer (<c>{Host}:{Port}/drive/...</c>).
    /// <paramref name="expireTime"/> chỉ để tương thích contract chung; không map sang TTL.
    /// </summary>
    /// <param name="bucket">
    /// Folder dưới RootPath. Rỗng = ngay dưới RootPath.
    /// </param>
    /// <param name="fileName">Path file tương đối trong bucket đó.</param>
    /// <param name="expireTime">Giữ tương thích API chung; không map sang TTL sharing link.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <example>
    /// <c>ViewAsync("invoices", "a.pdf")</c>
    /// → sharing URL DSM, hoặc fallback
    /// <c>https://nas:5001/drive/team-folders%2FERP%2Finvoices%2Fa.pdf</c>.
    /// </example>
    public virtual async Task<string> ViewAsync(
        string bucket,
        string fileName,
        int expireTime = 1800,
        CancellationToken cancellationToken = default)
    {
        var path = ToDrivePath(bucket, fileName);
        _logger.LogDebug(
            "View link for Synology path {Path}, expireSeconds={ExpireSeconds} (unused as TTL)",
            path,
            expireTime);

        var sharingUrl = await Client.CreateSharingLinkAsync(path, cancellationToken);
        if (!string.IsNullOrWhiteSpace(sharingUrl))
            return sharingUrl;

        var host = _options.Host.Trim();
        var port = SynologyBlobStoringDefaults.ResolvePort(_options);
        var useHttps = _options.UseHttps || port == 443;
        var scheme = useHttps ? "https" : "http";
        var authority = (useHttps ? port == 443 : port == 80) ? host : $"{host}:{port}";
        return $"{scheme}://{authority}/drive/{Uri.EscapeDataString(path.Trim('/'))}";
    }

    /// <summary>
    /// Liệt kê path file phẳng dưới <paramref name="bucket"/>,
    /// lọc thêm theo <paramref name="prefix"/> phía client.
    /// Folder không tồn tại → danh sách rỗng.
    /// </summary>
    /// <param name="bucket">
    /// Folder dưới RootPath cần liệt kê. Rỗng = ngay dưới RootPath.
    /// </param>
    /// <param name="prefix">Lọc thêm object key tương đối bucket (phía client).</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <example>
    /// <c>GetFileNamesAsync("invoices", "2024/")</c>
    /// → <c>["2024/a.pdf", "2024/b.pdf"]</c> (key tương đối bucket).
    /// </example>
    public virtual async Task<IReadOnlyList<string>> GetFileNamesAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        var folder = Normalize(bucket) ?? string.Empty;
        var drivePath = ToDriveFolderPath(folder);

        _logger.LogDebug(
            "Listing Synology bucket {Bucket}, prefix={Prefix}",
            folder,
            prefix ?? string.Empty);

        var names = new List<string>();
        try
        {
            await CollectFileNamesAsync(drivePath, names, cancellationToken);
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("failed with error code", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug(ex, "Synology folder missing for {Bucket}; empty list", folder);
            return [];
        }

        if (!string.IsNullOrEmpty(prefix))
            names = names.Where(n => n.StartsWith(prefix, StringComparison.Ordinal)).ToList();

        return names;
    }

    public virtual async Task RenameAsync(
        string bucket,
        string path,
        string newName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        if (string.IsNullOrEmpty(path))
        {
            var normalizedBucket = Normalize(bucket);
            if (!string.IsNullOrEmpty(normalizedBucket))
                EnsureNoTraversal(normalizedBucket);

            EnsureNoTraversal(newName);

            var drivePath = ToDriveFolderPath(normalizedBucket);
            _logger.LogDebug(
                "Renaming Synology bucket folder {Path} to {NewName}",
                drivePath,
                newName);

            await Client.RenameAsync(drivePath, newName, cancellationToken).ConfigureAwait(false);
            return;
        }

        var itemDrivePath = ToItemDrivePath(bucket, path);
        _logger.LogDebug(
            "Renaming Synology path {Path} to {NewName}",
            itemDrivePath,
            newName);

        await Client.RenameAsync(itemDrivePath, newName, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task MoveAsync(
        string bucket,
        string sourcePath,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        if (ObjectStoragePathHelper.IsDestinationInsideSource(sourcePath, destinationFolder))
        {
            throw new InvalidOperationException(
                "Cannot move an item into itself or one of its subfolders.");
        }

        var sourceDrive = ToItemDrivePath(bucket, sourcePath);
        var destinationParentDrive = ToDriveFolderPath(
            CombineDrivePath(Normalize(bucket), Normalize(destinationFolder) ?? string.Empty));

        _logger.LogDebug(
            "Moving Synology path {Source} to folder {DestinationFolder}",
            sourceDrive,
            destinationParentDrive);

        await Client.MoveAsync(sourceDrive, destinationParentDrive, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task MoveFilesAsync(
        string bucket,
        string destinationFolder,
        IEnumerable<string> sourcePaths,
        CancellationToken cancellationToken = default)
    {
        foreach (var sourcePath in ObjectStoragePathHelper.OrderForBatchMove(sourcePaths))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await MoveAsync(bucket, sourcePath, destinationFolder, cancellationToken).ConfigureAwait(false);
        }
    }

    private string ToItemDrivePath(string bucket, string relativePath)
    {
        var relative = CombineDrivePath(Normalize(bucket), Normalize(relativePath)!);
        EnsureNoTraversal(relative);
        return ToDriveFolderPath(relative);
    }

    /// <summary>
    /// Dựng node folder/file dưới <paramref name="bucket"/> trên Synology Drive
    /// (gốc <see cref="SynologyBlobOptions.RootPath"/>). Null/rỗng = RootPath.
    /// Lazy: chỉ liệt kê con trực tiếp; folder con có <c>Children</c> rỗng.
    /// Path thiếu vẫn trả node gốc với <c>Children</c> rỗng.
    /// </summary>
    /// <param name="bucket">
    /// Path folder tương đối RootPath (API Synology riêng, không qua tham số fileName của blob).
    /// </param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <example>
    /// <c>GetTreeAsync("invoices")</c>
    /// → node <c>Path="invoices"</c> với children trực tiếp (folder/file), không đệ quy sâu.
    /// </example>
    public virtual async Task<SynologyDriveNode> GetTreeAsync(
        string? bucket = null,
        CancellationToken cancellationToken = default)
    {
        var path = Normalize(bucket) ?? string.Empty;
        var name = string.IsNullOrEmpty(path)
            ? (RootPath?.Split('/').LastOrDefault() ?? "Synology Drive")
            : path.Split('/').Last();

        _logger.LogDebug(
            "Building Synology drive tree under {RootPath}",
            string.IsNullOrEmpty(path) ? "(storage root)" : path);

        var children = new List<SynologyDriveNode>();
        try
        {
            await CollectTreeChildrenAsync(path, children, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogDebug(ex, "Synology path not found for tree root {RootPath}; empty children", path);
        }

        return new SynologyDriveNode
        {
            Name = name,
            Path = path,
            IsFolder = true,
            Children = children
        };
    }

    /// <summary>Giải phóng <see cref="SynologyClient"/> nếu đã được tạo.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        if (_client.IsValueCreated)
            _client.Value.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Duyệt đệ quy (BFS) children, thu path file và chuyển thành object key tương đối bucket drive path.
    /// </summary>
    /// <example>
    /// Bucket drive <c>team-folders/ERP/invoices</c> chứa <c>.../invoices/2024/a.pdf</c>
    /// → thêm key <c>2024/a.pdf</c> vào <paramref name="names"/>.
    /// </example>
    private async Task CollectFileNamesAsync(
        string bucketDrivePath,
        List<string> names,
        CancellationToken cancellationToken)
    {
        var folders = new Queue<string>();
        folders.Enqueue(Client.NormalizePath(bucketDrivePath));

        while (folders.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = folders.Dequeue();

            IReadOnlyList<SynologyDriveItem> children;
            try
            {
                children = await Client.ListFolderAsync(current, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogDebug(ex, "Synology list failed for path {Path}", current);
                continue;
            }

            foreach (var item in children)
            {
                if (item.IsFolder)
                {
                    var childPath = Client.NormalizePath(
                        item.DisplayPath ?? item.Path ?? CombineDrivePath(current, item.Name ?? string.Empty));
                    if (!string.IsNullOrEmpty(childPath))
                        folders.Enqueue(childPath);
                    continue;
                }

                var fullPath = Normalize(item.DisplayPath ?? item.Path) ?? string.Empty;
                var key = ToObjectKey(fullPath, bucketDrivePath);
                if (string.IsNullOrEmpty(key))
                    key = Normalize(item.Name) ?? string.Empty;
                if (!string.IsNullOrEmpty(key))
                    names.Add(key);
            }
        }
    }

    /// <summary>
    /// Liệt kê children trực tiếp dưới <paramref name="bucketPath"/>.
    /// Không đệ quy — folder con trả về <c>Children</c> rỗng để client lazy-load từng cấp.
    /// </summary>
    /// <param name="bucketPath">Path tương đối RootPath; rỗng = storage root.</param>
    /// <param name="children">Accumulator nhận node con.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <example>
    /// <c>CollectTreeChildrenAsync("invoices", children, ct)</c>
    /// → thêm node <c>2024/</c> (folder, Children rỗng) và <c>a.pdf</c> (file) vào <paramref name="children"/>.
    /// </example>
    private async Task CollectTreeChildrenAsync(
        string bucketPath,
        List<SynologyDriveNode> children,
        CancellationToken cancellationToken)
    {
        var drivePath = ToDriveFolderPath(bucketPath);
        IReadOnlyList<SynologyDriveItem> items;
        try
        {
            items = await Client.ListFolderAsync(drivePath, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogDebug(ex, "Synology list failed for {Path}", drivePath);
            return;
        }

        foreach (var item in items.OrderBy(static i => i.Name, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(item.Name))
                continue;

            var childPath = CombineDrivePath(bucketPath, item.Name);
            if (item.IsFolder)
            {
                children.Add(new SynologyDriveNode
                {
                    Name = item.Name,
                    Path = childPath,
                    IsFolder = true,
                    Children = []
                });
                continue;
            }

            children.Add(new SynologyDriveNode
            {
                Name = item.Name,
                Path = childPath,
                IsFolder = false
            });
        }
    }

    /// <summary>
    /// Bảo đảm từng cấp của <paramref name="folderPath"/> tồn tại trên Drive
    /// (tạo tuần tự từ cha → con nếu thiếu). Conflict khi folder đã có được bỏ qua.
    /// Null/rỗng → no-op.
    /// </summary>
    /// <example>
    /// <c>EnsureFolderAsync("team-folders/ERP/invoices/2024")</c>
    /// → tạo lần lượt <c>team-folders</c> → <c>.../ERP</c> → <c>.../invoices</c> → <c>.../2024</c> nếu thiếu.
    /// </example>
    private async Task EnsureFolderAsync(string? folderPath, CancellationToken cancellationToken)
    {
        var normalized = Normalize(folderPath);
        if (string.IsNullOrEmpty(normalized))
            return;

        EnsureNoTraversal(normalized);

        string? current = null;
        foreach (var segment in normalized.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            current = CombineDrivePath(current, segment);

            var item = await Client.GetItemAsync(current, cancellationToken);
            if (item is not null)
                continue;

            try
            {
                await Client.CreateFolderAsync(current, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogDebug(ex, "Synology folder {FolderPath} may already exist", current);
            }
        }
    }

    /// <summary>
    /// Ghép <paramref name="bucket"/> + <paramref name="fileName"/> thành path Drive tuyệt đối (đã gồm RootPath).
    /// </summary>
    /// <example>
    /// RootPath=<c>team-folders/ERP</c><br/>
    /// <c>ToDrivePath("invoices", "2024/a.pdf")</c> → <c>team-folders/ERP/invoices/2024/a.pdf</c><br/>
    /// <c>ToDrivePath("", "a.pdf")</c> → <c>team-folders/ERP/a.pdf</c>
    /// </example>
    private string ToDrivePath(string bucket, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var relative = CombineDrivePath(Normalize(bucket), Normalize(fileName)!);
        EnsureNoTraversal(relative);
        return ToDriveFolderPath(relative);
    }

    /// <summary>Path folder tương đối RootPath → path Drive tuyệt đối.</summary>
    /// <example>
    /// RootPath=<c>team-folders/ERP</c><br/>
    /// <c>ToDriveFolderPath("invoices")</c> → <c>team-folders/ERP/invoices</c><br/>
    /// <c>ToDriveFolderPath(null)</c> → <c>team-folders/ERP</c>
    /// </example>
    private string ToDriveFolderPath(string? relativeBucket)
    {
        var relative = Normalize(relativeBucket);
        if (!string.IsNullOrEmpty(relative))
            EnsureNoTraversal(relative);

        if (string.IsNullOrEmpty(relative))
            return RootPath ?? string.Empty;

        return CombineDrivePath(RootPath, relative);
    }

    /// <summary>
    /// Ghép hai đoạn path bằng <c>/</c>.
    /// <paramref name="basePath"/> null/rỗng → trả <paramref name="relativePath"/>.
    /// </summary>
    /// <example>
    /// <c>CombineDrivePath("invoices", "2024/a.pdf")</c> → <c>invoices/2024/a.pdf</c><br/>
    /// <c>CombineDrivePath("invoices", "a.pdf")</c> → <c>invoices/a.pdf</c><br/>
    /// <c>CombineDrivePath(null, "a.pdf")</c> → <c>a.pdf</c><br/>
    /// <c>CombineDrivePath("invoices/2024", "report")</c> → <c>invoices/2024/report</c>
    /// </example>
    private static string CombineDrivePath(string? basePath, string relativePath) =>
        string.IsNullOrEmpty(basePath) ? relativePath : $"{basePath}/{relativePath}";

    /// <summary>
    /// Bỏ segment cuối của drive path → path folder chứa item đó.
    /// Path một segment → null.
    /// </summary>
    /// <example>
    /// <c>GetParentFolderPath("invoices/2024/a.pdf")</c> → <c>invoices/2024</c><br/>
    /// <c>GetParentFolderPath("invoices/a.pdf")</c> → <c>invoices</c><br/>
    /// <c>GetParentFolderPath("a.pdf")</c> → <c>null</c><br/>
    /// <c>GetParentFolderPath("invoices/2024")</c> → <c>invoices</c>
    /// </example>
    private static string? GetParentFolderPath(string drivePath)
    {
        if (string.IsNullOrEmpty(drivePath))
            return null;

        var lastSlash = drivePath.LastIndexOf('/');
        return lastSlash <= 0 ? null : drivePath[..lastSlash];
    }

    /// <summary>
    /// Đổi path Drive tuyệt đối thành object key tương đối <paramref name="folderRoot"/>
    /// (contract <see cref="GetFileNamesAsync"/>).
    /// </summary>
    /// <example>
    /// <c>ToObjectKey("team-folders/ERP/invoices/2024/a.pdf", "team-folders/ERP/invoices")</c>
    /// → <c>2024/a.pdf</c><br/>
    /// <c>ToObjectKey("invoices/a.pdf", "invoices")</c> → <c>a.pdf</c><br/>
    /// <c>ToObjectKey("invoices/2024/a.pdf", null)</c> → <c>invoices/2024/a.pdf</c><br/>
    /// <c>ToObjectKey("invoices", "invoices")</c> → <c>""</c>
    /// </example>
    private static string ToObjectKey(string drivePath, string? folderRoot)
    {
        var path = Normalize(drivePath) ?? string.Empty;
        var root = Normalize(folderRoot);

        if (string.IsNullOrEmpty(root))
            return path;

        var prefix = root + "/";
        if (path.StartsWith(prefix, StringComparison.Ordinal))
            return path[prefix.Length..];

        if (string.Equals(path, root, StringComparison.Ordinal))
            return string.Empty;

        return path;
    }

    /// <summary>
    /// Chuẩn hóa segment path: đổi <c>\</c> → <c>/</c>, trim <c>/</c> đầu/cuối.
    /// Null/whitespace → null.
    /// </summary>
    /// <example>
    /// <c>Normalize("/invoices\\2024/")</c> → <c>invoices/2024</c><br/>
    /// <c>Normalize("  ")</c> → <c>null</c>
    /// </example>
    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Replace('\\', '/').Trim('/');
    }

    /// <summary>
    /// Từ chối path chứa segment <c>.</c> hoặc <c>..</c> (path traversal).
    /// </summary>
    /// <example>
    /// <c>EnsureNoTraversal("invoices/../etc")</c> → throw<br/>
    /// <c>EnsureNoTraversal("invoices/2024")</c> → ok
    /// </example>
    private static void EnsureNoTraversal(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (path.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(static s => s is ".." or "."))
            throw new InvalidOperationException("Blob path segments must not contain '.' or '..'.");
    }
}
