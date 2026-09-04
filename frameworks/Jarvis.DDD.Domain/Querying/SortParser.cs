namespace Jarvis.DDD.Domain.Querying;

public static class SortParser
{
    public static IReadOnlyList<SortField> Parse(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return Array.Empty<SortField>();

        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<SortField>(fields.Length);

        foreach (var field in fields)
        {
            var parts = field.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var fieldName = parts[0];
            var direction = SortDirection.Asc; // Default

            if (parts.Length > 1)
            {
                if (string.Equals(parts[1], "asc", StringComparison.OrdinalIgnoreCase))
                {
                    direction = SortDirection.Asc;
                }
                else if (string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase))
                {
                    direction = SortDirection.Desc;
                }
                else
                {
                    throw new ArgumentException($"Invalid sort direction '{parts[1]}' for field '{fieldName}'. Expected 'asc' or 'desc'.");
                }
            }

            result.Add(new SortField(fieldName, direction));
        }

        return result;
    }
}
