namespace Jarvis.DDD.Domain.Querying;

public class InvalidFilterException : ArgumentException
{
    public InvalidFilterException(string message) : base(message)
    {
    }

    public InvalidFilterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
