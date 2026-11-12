namespace Platform.DDD.Domain.Shared.Attributes;

/// <summary>
/// Endpoint không chạy middleware có tên trùng <see cref="Name"/> (ví dụ <c>MultiOrgMiddleware</c>).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class IgnoreMiddlewareAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}
