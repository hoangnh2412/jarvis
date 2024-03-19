namespace Jarvis.Domain.Attributes;

/// <summary>
/// Attribute to define the field with String data type is Code
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class CodeAttribute : Attribute { }