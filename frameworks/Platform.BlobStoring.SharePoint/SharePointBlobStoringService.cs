using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateLink;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Platform.BlobStoring.Helpers;

namespace Platform.BlobStoring.SharePoint;

/// <summary>
/// Lưu blob trên SharePoint Online Document Library qua Microsoft Graph.
/// Đồng thời hiện thực <see cref="IBlobStoringService"/> và <see cref="ISharePointFolderService"/>.
/// </summary>
/// <remarks>
/// <para>
/// Hiện thực <see cref="IBlobStoringService"/> (tham số contract tên <c>bucket</c>)
/// nhưng trong class này dùng tên <c>folder</c> cho đúng ngữ nghĩa SharePoint:
/// </para>
/// <list type="bullet">
/// <item>
/// <term>DriveId</term>
/// <description>
/// Document Library (Graph drive) do host cấu hình trong <see cref="SharePointBlobOptions.DriveId"/>.
/// Mọi thao tác gọi <c>/drives/{DriveId}/...</c>; package không resolve từ site/library.
/// </description>
/// </item>
/// <item>
/// <term>folder</term>
/// <description>
/// Tương đương tham số <c>bucket</c> của contract chung; trên SharePoint = đường dẫn thư mục
/// dưới library root (vd. <c>invoices</c> hoặc <c>invoices/2024</c>). Rỗng = library root.
/// </description>
/// </item>
/// <item>
/// <term>fileName</term>
/// <description>
/// Đường dẫn file tương đối trong folder đó (object key), ví dụ <c>a.pdf</c> hoặc <c>2024/a.pdf</c>.
/// Path Graph đầy đủ: <c>/drives/{DriveId}/root:/{folder}/{fileName}:</c>.
/// </description>
/// </item>
/// </list>
/// </remarks>
public class SharePointBlobStoringService : IBlobStoringService, ISharePointFolderService
{
    /// <summary>OAuth scope app-only cho Microsoft Graph (client credentials).</summary>
    private static readonly string[] GraphScopes = ["https://graph.microsoft.com/.default"];

    private readonly SharePointBlobOptions _options;
    private readonly Lazy<GraphServiceClient> _graphClient;
    private readonly ILogger<SharePointBlobStoringService> _logger;

    /// <summary>Khởi tạo service từ Options và logger do DI cung cấp.</summary>
    public SharePointBlobStoringService(
        IOptions<SharePointBlobOptions> options,
        ILogger<SharePointBlobStoringService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _graphClient = new Lazy<GraphServiceClient>(CreateClient);

        if (string.Equals(_options.ViewLinkScope, "anonymous", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "SharePoint ViewLinkScope is 'anonymous'; view links may be accessible outside the organization.");
        }
    }

    /// <summary>Graph client dùng chung; tạo lần đầu khi cần.</summary>
    private GraphServiceClient Graph => _graphClient.Value;

    /// <summary>
    /// Document Library (Graph drive) do host cấu hình; mọi folder/file nằm dưới drive này.
    /// </summary>
    private string DriveId => _options.DriveId;

    /// <summary>
    /// Tạo <see cref="GraphServiceClient"/> bằng Client Credentials (TenantId, ClientId, ClientSecret).
    /// </summary>
    private GraphServiceClient CreateClient()
    {
        _logger.LogDebug("Creating GraphServiceClient for SharePoint drive {DriveId}", _options.DriveId);

        var credential = new ClientSecretCredential(
            _options.TenantId,
            _options.ClientId,
            _options.ClientSecret);

        return new GraphServiceClient(credential, GraphScopes);
    }

    /// <summary>
    /// Upload file lên Document Library (drive <see cref="DriveId"/>).
    /// Ghép <paramref name="folder"/>/<paramref name="fileName"/> thành path file;
    /// ensure folder cha của path đó nếu thiếu; upload bằng Graph upload session (mọi kích thước).
    /// </summary>
    /// <param name="folder">
    /// Folder dưới Document Library root (tương đương <c>bucket</c> trên <see cref="IBlobStoringService"/>).
    /// Rỗng = library root. Ví dụ <c>invoices</c>.
    /// </param>
    /// <param name="fileName">Path file tương đối trong folder đó (vd. <c>2024/a.pdf</c>).</param>
    /// <param name="bytes">Nội dung file.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task UploadAsync(
        string folder,
        string fileName,
        byte[] bytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        await using var stream = new MemoryStream(bytes);
        await UploadStreamAsync(folder, fileName, stream, bytes.Length, cancellationToken);
    }

    public virtual async Task UploadStreamAsync(
        string folder,
        string fileName,
        Stream content,
        long? contentLength = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var path = CombineDrivePath(folder, fileName);
        await EnsureFolderAsync(GetParentFolderPath(path), cancellationToken);
        _logger.LogDebug(
            "Uploading stream for {FileName} to SharePoint path {Path}, contentLength={ContentLength}",
            fileName,
            path,
            contentLength?.ToString() ?? "(unknown)");

        var uploadSession = await Graph.Drives[DriveId].Root.ItemWithPath(path).CreateUploadSession
            .PostAsync(
                new CreateUploadSessionPostRequestBody
                {
                    Item = new DriveItemUploadableProperties
                    {
                        AdditionalData = new Dictionary<string, object>
                        {
                            ["@microsoft.graph.conflictBehavior"] = "replace"
                        }
                    }
                },
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Failed to create upload session for '{path}'.");

        var uploadTask = new LargeFileUploadTask<DriveItem>(
            uploadSession,
            content,
            maxSliceSize: 320 * 1024,
            Graph.RequestAdapter);

        var result = await uploadTask.UploadAsync(cancellationToken: cancellationToken);
        if (!result.UploadSucceeded)
            throw new InvalidOperationException($"Upload failed for SharePoint path '{path}'.");
    }

    public virtual async Task<byte[]> DownloadAsync(
        string folder,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        await using var result = await OpenReadAsync(folder, fileName, cancellationToken);
        return await BlobStoringStreamHelper.ReadAllBytesAsync(result.Content, cancellationToken);
    }

    public virtual async Task<BlobReadResult> OpenReadAsync(
        string folder,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var path = CombineDrivePath(folder, fileName);

        _logger.LogDebug("Opening read stream for {FileName} from SharePoint path {Path}", fileName, path);

        var content = await Graph.Drives[DriveId].Root.ItemWithPath(path).Content
            .GetAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"File content was not returned for SharePoint path '{path}'.");

        return BlobReadResult.FromStream(
            content,
            contentType: "application/octet-stream",
            contentLength: BlobStoringStreamHelper.ResolveUploadSize(content, null));
    }

    /// <summary>
    /// Tạo folder bucket rỗng dưới Document Library. Idempotent khi folder đã tồn tại.
    /// Khi <paramref name="path"/> có giá trị, tạo folder phân cấp dưới <paramref name="folder"/>.
    /// </summary>
    /// <param name="folder">
    /// Folder dưới Document Library root (tương đương <c>bucket</c> trên contract chung). Rỗng = library root.
    /// </param>
    /// <param name="path">Folder con tương đối trong <paramref name="folder"/> (vd. <c>2024/Q1</c>).</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task CreateBucketAsync(
        string folder,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(folder))
            EnsureNoTraversal(folder);

        var relativePath = string.IsNullOrWhiteSpace(path)
            ? null
            : path.Trim().Trim('/');

        if (!string.IsNullOrEmpty(relativePath))
            EnsureNoTraversal(relativePath);

        var folderPath = string.IsNullOrEmpty(relativePath)
            ? folder
            : CombineDrivePath(folder, relativePath);

        _logger.LogDebug(
            "Creating SharePoint folder bucket {Folder}",
            string.IsNullOrEmpty(folderPath) ? "(library root)" : folderPath);

        await EnsureFolderAsync(folderPath, cancellationToken);
    }

    /// <summary>
    /// Xóa một file (hoặc item) theo path trong Document Library.
    /// Lỗi Graph (kể cả 404) được trả về caller.
    /// </summary>
    /// <param name="folder">
    /// Folder dưới Document Library root (tương đương <c>bucket</c> trên contract chung). Rỗng = library root.
    /// </param>
    /// <param name="fileName">Path item tương đối trong folder đó.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task DeleteFileAsync(
        string folder,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var path = CombineDrivePath(folder, fileName);

        _logger.LogDebug("Deleting {FileName} from SharePoint path {Path}", fileName, path);

        await Graph.Drives[DriveId].Root.ItemWithPath(path)
            .DeleteAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Xóa tuần tự nhiều file trong cùng <paramref name="folder"/>.
    /// Không lọc tên; host chịu trách nhiệm input.
    /// </summary>
    /// <param name="folder">
    /// Folder dưới Document Library root (tương đương <c>bucket</c> trên contract chung). Rỗng = library root.
    /// </param>
    /// <param name="fileNames">Danh sách path file tương đối trong folder đó.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task DeleteFilesAsync(
        string folder,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken = default)
    {
        foreach (var name in fileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DeleteFileAsync(folder, name, cancellationToken);
        }
    }

    /// <summary>
    /// Xóa folder bucket và toàn bộ nội dung bên trong. Folder không tồn tại → no-op.
    /// Không cho phép xóa library root.
    /// </summary>
    /// <param name="folder">
    /// Folder dưới Document Library root (tương đương <c>bucket</c> trên contract chung).
    /// </param>
    /// <param name="prefix">Subtree tương đối trong folder bucket; null/empty = xóa toàn bộ bucket.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task DeleteBucketAsync(
        string folder,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(folder) && string.IsNullOrEmpty(prefix))
            throw new InvalidOperationException("Cannot delete the SharePoint document library root.");

        if (!string.IsNullOrEmpty(folder))
            EnsureNoTraversal(folder);
        if (!string.IsNullOrEmpty(prefix))
            EnsureNoTraversal(prefix);

        var path = string.IsNullOrEmpty(prefix) ? folder : CombineDrivePath(folder, prefix);
        if (string.IsNullOrEmpty(path))
            throw new InvalidOperationException("Cannot delete the SharePoint document library root.");

        _logger.LogDebug("Deleting SharePoint folder bucket {Path}", path);

        try
        {
            await Graph.Drives[DriveId].Root.ItemWithPath(path)
                .DeleteAsync(cancellationToken: cancellationToken);
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == 404)
        {
            _logger.LogDebug("SharePoint folder not found for delete {Path}; treating as no-op", path);
        }
    }

    /// <summary>
    /// Tạo URL xem file trên SharePoint bằng Graph <c>createLink</c> (type=view, scope từ options).
    /// <paramref name="expireTime"/> chỉ để tương thích contract chung; Graph createLink không map TTL.
    /// Không fallback sang webUrl/preview — lỗi Graph trả về caller.
    /// </summary>
    /// <param name="folder">
    /// Folder dưới Document Library root (tương đương <c>bucket</c> trên contract chung). Rỗng = library root.
    /// </param>
    /// <param name="fileName">Path file tương đối trong folder đó.</param>
    /// <param name="expireTime">Giữ tương thích API chung; không map sang TTL createLink.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task<string> ViewAsync(
        string folder,
        string fileName,
        int expireTime = 1800,
        CancellationToken cancellationToken = default)
    {
        var path = CombineDrivePath(folder, fileName);

        _logger.LogDebug(
            "Creating view link for SharePoint path {Path}, expireSeconds={ExpireSeconds} (not used by createLink)",
            path,
            expireTime);

        var scope = string.IsNullOrWhiteSpace(_options.ViewLinkScope)
            ? "organization"
            : _options.ViewLinkScope;

        if (string.Equals(scope, "anonymous", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Creating anonymous SharePoint view link for path {Path}; expireSeconds={ExpireSeconds} is not applied by Graph createLink.",
                path,
                expireTime);
        }

        var permission = await Graph.Drives[DriveId].Root.ItemWithPath(path).CreateLink
            .PostAsync(
                new CreateLinkPostRequestBody
                {
                    Type = "view",
                    Scope = scope
                },
                cancellationToken: cancellationToken);

        return permission?.Link?.WebUrl
            ?? throw new InvalidOperationException($"View URL was not returned for SharePoint path '{path}'.");
    }

    /// <summary>
    /// Liệt kê path file phẳng dưới <paramref name="folder"/>,
    /// lọc thêm theo <paramref name="prefix"/> phía client.
    /// Folder không tồn tại (404) → danh sách rỗng.
    /// </summary>
    /// <param name="folder">
    /// Folder dưới library root cần liệt kê (tương đương <c>bucket</c> trên contract chung). Rỗng = library root.
    /// </param>
    /// <param name="prefix">Lọc thêm object key tương đối folder (phía client).</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task<IReadOnlyList<string>> GetFileNamesAsync(
        string folder,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(folder))
            EnsureNoTraversal(folder);

        _logger.LogDebug(
            "Listing SharePoint files under folder {Folder}, prefix={Prefix}",
            folder,
            prefix ?? string.Empty);

        var names = new List<string>();
        try
        {
            await CollectFileNamesAsync(folder, names, cancellationToken);
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == 404)
        {
            _logger.LogDebug(
                "SharePoint folder not found for {Folder}; returning empty list",
                folder);
            return [];
        }

        if (!string.IsNullOrEmpty(prefix))
            names = names.Where(n => n.StartsWith(prefix, StringComparison.Ordinal)).ToList();

        _logger.LogDebug(
            "Listed {Count} file(s) under SharePoint folder {Folder}, prefix={Prefix}",
            names.Count,
            folder,
            prefix ?? string.Empty);

        return names;
    }

    public virtual async Task RenameAsync(
        string folder,
        string path,
        string newName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        if (string.IsNullOrEmpty(path))
        {
            if (!string.IsNullOrEmpty(folder))
                EnsureNoTraversal(folder);

            EnsureNoTraversal(newName);

            _logger.LogDebug(
                "Renaming SharePoint bucket folder {Folder} to {NewName}",
                string.IsNullOrEmpty(folder) ? "(library root)" : folder,
                newName);

            await Graph.Drives[DriveId].Root.ItemWithPath(folder)
                .PatchAsync(new DriveItem { Name = newName }, cancellationToken: cancellationToken);
            return;
        }

        var drivePath = CombineDrivePath(folder, path);
        _logger.LogDebug(
            "Renaming SharePoint path {Path} to {NewName}",
            drivePath,
            newName);

        await Graph.Drives[DriveId].Root.ItemWithPath(drivePath)
            .PatchAsync(new DriveItem { Name = newName }, cancellationToken: cancellationToken);
    }

    public virtual async Task MoveAsync(
        string folder,
        string sourcePath,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        if (ObjectStoragePathHelper.IsDestinationInsideSource(sourcePath, destinationFolder))
        {
            throw new InvalidOperationException(
                "Cannot move an item into itself or one of its subfolders.");
        }

        var source = CombineDrivePath(folder, sourcePath);
        var parent = CombineDrivePath(folder, destinationFolder);

        _logger.LogDebug(
            "Moving SharePoint path {Source} to folder {DestinationFolder}",
            source,
            string.IsNullOrEmpty(parent) ? "(library root)" : parent);

        await Graph.Drives[DriveId].Root.ItemWithPath(source)
            .PatchAsync(
                new DriveItem
                {
                    ParentReference = new ItemReference
                    {
                        Path = string.IsNullOrEmpty(parent)
                            ? $"/drives/{DriveId}/root:"
                            : $"/drives/{DriveId}/root:/{parent}:"
                    }
                },
                cancellationToken: cancellationToken);
    }

    public virtual async Task MoveFilesAsync(
        string folder,
        string destinationFolder,
        IEnumerable<string> sourcePaths,
        CancellationToken cancellationToken = default)
    {
        foreach (var sourcePath in ObjectStoragePathHelper.OrderForBatchMove(sourcePaths))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await MoveAsync(folder, sourcePath, destinationFolder, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Dựng node folder/file dưới <paramref name="rootPath"/> trong Document Library
    /// (drive <see cref="DriveId"/>). Null/rỗng = library root.
    /// Lazy: chỉ liệt kê con trực tiếp; folder con có <c>Children</c> rỗng.
    /// Path 404 vẫn trả node gốc với <c>Children</c> rỗng.
    /// </summary>
    /// <param name="rootPath">
    /// Path folder tương đối library root (API SharePoint riêng, không qua tham số folder của blob).
    /// </param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public virtual async Task<SharePointDriveNode> GetTreeAsync(
        string? rootPath = null,
        CancellationToken cancellationToken = default)
    {
        var path = rootPath ?? string.Empty;
        EnsureNoTraversal(path);
        var name = string.IsNullOrEmpty(path)
            ? "root"
            : path.Split('/').Last();

        _logger.LogDebug("Building SharePoint drive tree under {RootPath}", string.IsNullOrEmpty(path) ? "(library root)" : path);

        var children = new List<SharePointDriveNode>();
        try
        {
            await CollectTreeChildrenAsync(string.IsNullOrEmpty(path) ? null : path, children, cancellationToken);
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == 404)
        {
            _logger.LogDebug("SharePoint path not found for tree root {RootPath}; returning empty children", path);
        }

        return new SharePointDriveNode
        {
            Name = name,
            Path = path,
            IsFolder = true,
            Children = children
        };
    }

    /// <summary>
    /// Liệt kê children trực tiếp (có phân trang @odata.nextLink) dưới <paramref name="folderDrivePath"/>.
    /// Không đệ quy — folder con trả về <c>Children</c> rỗng để client lazy-load từng cấp.
    /// </summary>
    /// <param name="folderDrivePath">Path folder hiện tại dưới library; null/rỗng = Document Library root.</param>
    /// <param name="children">Accumulator nhận node con của folder hiện tại.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    private async Task CollectTreeChildrenAsync(
        string? folderDrivePath,
        List<SharePointDriveNode> children,
        CancellationToken cancellationToken)
    {
        DriveItemCollectionResponse? page;
        if (string.IsNullOrEmpty(folderDrivePath))
        {
            page = await Graph.Drives[DriveId].Items["root"].Children
                .GetAsync(cancellationToken: cancellationToken);
        }
        else
        {
            page = await Graph.Drives[DriveId].Root.ItemWithPath(folderDrivePath).Children
                .GetAsync(cancellationToken: cancellationToken);
        }

        while (page is not null)
        {
            foreach (var item in page.Value ?? [])
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                    continue;

                var childPath = CombineDrivePath(folderDrivePath, item.Name);

                if (item.Folder is not null)
                {
                    children.Add(new SharePointDriveNode
                    {
                        Name = item.Name,
                        Path = childPath,
                        IsFolder = true,
                        Children = []
                    });
                    continue;
                }

                if (item.File is null)
                    continue;

                children.Add(new SharePointDriveNode
                {
                    Name = item.Name,
                    Path = childPath,
                    IsFolder = false
                });
            }

            if (string.IsNullOrEmpty(page.OdataNextLink))
                break;

            page = await Graph.Drives[DriveId].Items["root"].Children
                .WithUrl(page.OdataNextLink)
                .GetAsync(cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Bảo đảm từng cấp của <paramref name="folderPath"/> tồn tại dưới Document Library
    /// (tạo tuần tự từ cha → con nếu thiếu). Conflict 409/405 khi folder đã có được bỏ qua.
    /// Null/rỗng → no-op.
    /// </summary>
    /// <param name="folderPath">Path folder tương đối library root (vd. <c>invoices/2024</c>), không gồm tên file.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    private async Task EnsureFolderAsync(
        string? folderPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(folderPath))
            return;

        var segments = folderPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? current = null;

        foreach (var segment in segments)
        {
            var parent = current;
            current = CombineDrivePath(current, segment);

            if (await FolderExistsAsync(current, cancellationToken))
                continue;

            _logger.LogDebug("Creating SharePoint folder {FolderPath}", current);

            var driveItem = new DriveItem
            {
                Name = segment,
                Folder = new Folder(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["@microsoft.graph.conflictBehavior"] = "fail"
                }
            };

            try
            {
                if (string.IsNullOrEmpty(parent))
                {
                    await Graph.Drives[DriveId].Items["root"].Children
                        .PostAsync(driveItem, cancellationToken: cancellationToken);
                }
                else
                {
                    await Graph.Drives[DriveId].Root.ItemWithPath(parent).Children
                        .PostAsync(driveItem, cancellationToken: cancellationToken);
                }
            }
            catch (ApiException ex) when (ex.ResponseStatusCode is 409 or 405)
            {
                _logger.LogDebug(ex, "SharePoint folder {FolderPath} already exists (conflict)", current);
            }
        }
    }

    /// <summary>
    /// Kiểm tra path tồn tại và là folder (có Folder facet). Rỗng → <c>true</c> (library root).
    /// 404 → <c>false</c>.
    /// </summary>
    /// <param name="folderPath">Path folder tương đối library root.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    private async Task<bool> FolderExistsAsync(
        string folderPath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(folderPath))
            return true;

        try
        {
            var item = await Graph.Drives[DriveId].Root.ItemWithPath(folderPath)
                .GetAsync(
                    requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Select = ["id", "folder"];
                    },
                    cancellationToken: cancellationToken);

            return item?.Folder is not null;
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == 404)
        {
            return false;
        }
    }

    /// <summary>
    /// Duyệt đệ quy children (có phân trang), thu path file và chuyển thành object key tương đối <paramref name="folderRoot"/>.
    /// </summary>
    /// <param name="folderRoot">
    /// Folder gốc cần liệt kê; null/rỗng = library root.
    /// Dùng vừa để bắt đầu duyệt vừa để cắt object key trả về.
    /// </param>
    /// <param name="names">Accumulator danh sách path file tương đối folder.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    private async Task CollectFileNamesAsync(
        string? folderRoot,
        List<string> names,
        CancellationToken cancellationToken)
    {
        var root = string.IsNullOrEmpty(folderRoot) ? null : folderRoot;
        await WalkAsync(root);

        async Task WalkAsync(string? folderDrivePath)
        {
            DriveItemCollectionResponse? page;
            if (string.IsNullOrEmpty(folderDrivePath))
            {
                page = await Graph.Drives[DriveId].Items["root"].Children
                    .GetAsync(cancellationToken: cancellationToken);
            }
            else
            {
                page = await Graph.Drives[DriveId].Root.ItemWithPath(folderDrivePath).Children
                    .GetAsync(cancellationToken: cancellationToken);
            }

            while (page is not null)
            {
                foreach (var item in page.Value ?? [])
                {
                    if (string.IsNullOrWhiteSpace(item.Name))
                        continue;

                    var childDrivePath = CombineDrivePath(folderDrivePath, item.Name);

                    if (item.Folder is not null)
                    {
                        await WalkAsync(childDrivePath);
                        continue;
                    }

                    if (item.File is null)
                        continue;

                    names.Add(ToObjectKey(childDrivePath, root));
                }

                if (string.IsNullOrEmpty(page.OdataNextLink))
                    break;

                page = await Graph.Drives[DriveId].Items["root"].Children
                    .WithUrl(page.OdataNextLink)
                    .GetAsync(cancellationToken: cancellationToken);
            }
        }
    }

    /// <summary>
    /// Ghép hai đoạn path dưới Document Library bằng <c>/</c>.
    /// <paramref name="basePath"/> null/rỗng → trả <paramref name="relativePath"/>.
    /// </summary>
    private static string CombineDrivePath(string? basePath, string relativePath)
    {
        EnsureNoTraversal(basePath);
        EnsureNoTraversal(relativePath);

        if (string.IsNullOrEmpty(relativePath))
            return basePath ?? string.Empty;

        var combined = string.IsNullOrEmpty(basePath) ? relativePath : $"{basePath}/{relativePath}";
        EnsureNoTraversal(combined);
        return combined;
    }

    /// <summary>Từ chối path chứa segment <c>.</c> hoặc <c>..</c>.</summary>
    private static void EnsureNoTraversal(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (path.Replace('\\', '/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Any(static segment => segment is ".." or "."))
        {
            throw new InvalidOperationException("Blob path segments must not contain '.' or '..'.");
        }
    }

    /// <summary>
    /// Bỏ segment cuối của drive path → path folder chứa item đó.
    /// Path một segment (item ngay dưới library root) → null.
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
    /// Đổi path dưới library thành object key tương đối <paramref name="folderRoot"/>
    /// (contract <see cref="GetFileNamesAsync"/>).
    /// </summary>
    /// <example>
    /// <c>ToObjectKey("invoices/2024/a.pdf", "invoices")</c> → <c>2024/a.pdf</c><br/>
    /// <c>ToObjectKey("invoices/a.pdf", "invoices")</c> → <c>a.pdf</c><br/>
    /// <c>ToObjectKey("invoices/2024/a.pdf", null)</c> → <c>invoices/2024/a.pdf</c><br/>
    /// <c>ToObjectKey("invoices", "invoices")</c> → <c>""</c>
    /// </example>
    private static string ToObjectKey(string drivePath, string? folderRoot)
    {
        if (string.IsNullOrEmpty(folderRoot))
            return drivePath;

        var prefix = folderRoot + "/";
        if (drivePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return drivePath[prefix.Length..];

        if (string.Equals(drivePath, folderRoot, StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return drivePath;
    }
}
