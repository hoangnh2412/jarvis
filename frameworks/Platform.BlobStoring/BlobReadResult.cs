namespace Platform.BlobStoring;

/// <summary>Readable blob payload returned by <see cref="IBlobStoringService.OpenReadAsync"/>.</summary>
public sealed class BlobReadResult : IAsyncDisposable
{
    public required Stream Content { get; init; }

    public string? ContentType { get; init; }

    public long? ContentLength { get; init; }

    public static BlobReadResult FromStream(
        Stream content,
        string? contentType = null,
        long? contentLength = null,
        Func<ValueTask>? disposeAsync = null) =>
        new()
        {
            Content = disposeAsync is null
                ? content
                : new DisposeOnCloseStream(content, disposeAsync),
            ContentType = contentType,
            ContentLength = contentLength
        };

    public ValueTask DisposeAsync() => Content.DisposeAsync();

    private sealed class DisposeOnCloseStream(Stream inner, Func<ValueTask> disposeAsync) : Stream
    {
        private bool _disposed;

        public override bool CanRead => inner.CanRead;

        public override bool CanSeek => inner.CanSeek;

        public override bool CanWrite => inner.CanWrite;

        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Flush() => inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

        public override void SetLength(long value) => inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            inner.ReadAsync(buffer, offset, count, cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            inner.ReadAsync(buffer, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCore().AsTask().GetAwaiter().GetResult();

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await DisposeCore().ConfigureAwait(false);
            await base.DisposeAsync().ConfigureAwait(false);
        }

        private async ValueTask DisposeCore()
        {
            if (_disposed)
                return;

            _disposed = true;
            await inner.DisposeAsync().ConfigureAwait(false);
            await disposeAsync().ConfigureAwait(false);
        }
    }
}
