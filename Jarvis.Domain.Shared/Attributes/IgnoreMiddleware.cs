namespace Jarvis.Domain.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class IgnoreMiddleware(string name) : Attribute
{
    public string Name { get; set; } = name;
}