namespace Jarvis.Mvc.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class IgnoreMiddlewareAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}