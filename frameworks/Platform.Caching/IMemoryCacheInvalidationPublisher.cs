namespace Platform.Caching;

public interface IMemoryCacheInvalidationPublisher
{
    Task PublishAsync(MemoryCacheInvalidationInfo info, CancellationToken cancellationToken = default);
}
