namespace Platform.BlobStoring.Helpers;

/// <summary>Shared stream helpers for blob providers.</summary>
public static class BlobStoringStreamHelper
{
    /// <summary>
    /// Resolves upload size for object-storage providers. Prefers explicit <paramref name="contentLength"/>,
    /// then seekable <paramref name="content"/> length.
    /// </summary>
    public static long? ResolveUploadSize(Stream content, long? contentLength)
    {
        if (contentLength is > 0)
            return contentLength;

        if (content.CanSeek)
        {
            var remaining = content.Length - content.Position;
            return remaining >= 0 ? remaining : null;
        }

        return null;
    }

    public static async Task<byte[]> ReadAllBytesAsync(Stream content, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (content is MemoryStream memory && memory.TryGetBuffer(out var segment))
            return segment.ToArray();

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        return buffer.ToArray();
    }
}
