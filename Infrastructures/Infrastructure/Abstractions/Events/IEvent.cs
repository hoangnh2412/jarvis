using System.Threading.Tasks;

namespace Infrastructure.Abstractions.Events
{
    public interface IEvent<T>
    {
        Task PublishAsync(T data);
    }
}