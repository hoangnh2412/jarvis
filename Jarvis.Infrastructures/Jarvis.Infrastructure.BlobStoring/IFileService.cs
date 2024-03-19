namespace Jarvis.Infrastructure.BlobStoring;

/// <summary>
/// The interface abstracts file handling functions
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Upload file
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileName">Path to save file</param>
    /// <param name="bytes">Data of file</param>
    /// <returns></returns>
    Task UploadAsync(string bucket, string fileName, byte[] bytes);

    /// <summary>
    /// Download file to bytes
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileName">Path file</param>
    /// <returns></returns>
    Task<byte[]> DownloadAsync(string bucket, string fileName);

    /// <summary>
    /// Delete single file
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileName">Path file</param>
    /// <returns></returns>
    Task DeleteAsync(string bucket, string fileName);

    /// <summary>
    /// Delete multiple files
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileName">Path file</param>
    /// <returns></returns>
    Task DeletesAsync(string bucket, IEnumerable<string> fileNames);

    /// <summary>
    /// Return the link to view file without download
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileName">Path file</param>
    /// <param name="expireTime">Time to live of link</param>
    /// <returns></returns>
    Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800);

    /// <summary>
    /// Return a list of paths that matching the prefix
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="prefix">Prefix to search file, include path of file</param>
    /// <returns></returns>
    IEnumerable<string> GetFileNames(string bucket, string prefix = null);
}