namespace Jarvis.Caching;

public interface IMemoryCacheInvalidatorPublisher
{
    Task PublishAsync(MemoryCacheInvalidationInfo info);
}