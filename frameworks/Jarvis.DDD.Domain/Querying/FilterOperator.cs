namespace Jarvis.DDD.Domain.Querying;

public enum FilterOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    Between,
    In,
    IsNull,
    IsNotNull
}
