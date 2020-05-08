namespace Infrastructure.Abstractions
{
    public interface IAuthorizationCrudPolicy : IAuthorizationPolicy
    {
    }

    public interface IAuthorizationCrudPolicy<T> : IAuthorizationCrudPolicy
    {
    }
}
