using System.Threading.Tasks;

namespace Infrastructure.Abstractions.ObserverPattern
{
    public interface ISubcriber
    {
    }

    public interface ISubcriber<T> : ISubcriber
    {
        Task HandleAsync(T data);
    }
}