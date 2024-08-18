
namespace Jarvis.Caching;

public interface IMemoryCacheInvalidator
{
    Task RemoveAsync(MemoryCacheInvalidationInfo info, CancellationToken cancellationToken = default);
}