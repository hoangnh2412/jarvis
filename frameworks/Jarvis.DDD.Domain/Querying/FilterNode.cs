// Represents the immutable Abstract Syntax Tree (AST) for dynamic filter expressions.
namespace Jarvis.DDD.Domain.Querying;

public abstract record FilterNode;

public sealed record FilterCondition(string FieldName, FilterOperator Operator, object? Value) : FilterNode;

public sealed record FilterGroup(LogicOperator Logic, IReadOnlyList<FilterNode> Nodes) : FilterNode;
