namespace Infrastructure.Abstractions.ObserverPattern
{
    public interface ISubjectFactory
    {
        ISubject<T> GetSubject<T>();
    }
}