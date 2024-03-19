namespace Jarvis.Application.Validations;

public interface IValidationContext
{
    void AddItem<T>(string key, T value);

    T GetItem<T>(string key);

    T GetOrAddItem<T>(string key, Func<T> builder);

    Task<T> GetOrAddItemAsync<T>(string key, Func<Task<T>> builder);
}