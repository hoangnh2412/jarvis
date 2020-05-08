using System.Threading.Tasks;

namespace Infrastructure.Abstractions.ObserverPattern
{
    public interface ISubject
    {
        
    }

    public interface ISubject<T> : ISubject
    {
        void Subcribe(ISubcriber<T> monitor);

        void Unsubcribe(ISubcriber<T> monitor);

        Task NotifyAsync(T topic);
    }
}