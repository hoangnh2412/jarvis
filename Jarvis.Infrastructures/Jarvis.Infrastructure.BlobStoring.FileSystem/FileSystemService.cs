namespace Jarvis.Infrastructure.BlobStoring.FileSystem;

public class FileSystemService : IFileService
{
    public virtual Task DeleteAsync(string bucket, string fileName)
    {
        var path = System.IO.Path.Combine(bucket, fileName);

        if (!System.IO.File.Exists(path))
            System.IO.File.Delete(path);

        return Task.CompletedTask;
    }

    public virtual Task DeletesAsync(string bucket, IEnumerable<string> fileNames)
    {
        foreach (var name in fileNames)
        {
            var path = System.IO.Path.Combine(bucket, name);

            if (!System.IO.File.Exists(name))
                System.IO.File.Delete(name);
        }

        return Task.CompletedTask;
    }

    public virtual async Task<byte[]> DownloadAsync(string bucket, string fileName)
    {
        var path = System.IO.Path.Combine(bucket, fileName);

        return await System.IO.File.ReadAllBytesAsync(path);
    }

    public virtual async Task UploadAsync(string bucket, string fileName, byte[] bytes)
    {
        var path = System.IO.Path.Combine(bucket, fileName);
        await System.IO.File.WriteAllBytesAsync(path, bytes);
    }

    public virtual Task<string> ViewAsync(string bucket, string fileName, int expireTime)
    {
        return Task.FromResult("");
    }

    public virtual IEnumerable<string> GetFileNames(string bucket, string prefix = null)
    {
        return null;
    }
}