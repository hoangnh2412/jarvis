namespace Platform.DDD.Domain.Querying;

public sealed record SortField(string FieldName, SortDirection Direction);