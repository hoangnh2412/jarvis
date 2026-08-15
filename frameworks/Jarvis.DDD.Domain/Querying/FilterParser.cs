// Pipeline Step 1 (Parsing): Parses incoming JSON strings into a secure AST for filtering.
using System.Text.Json;

namespace Jarvis.DDD.Domain.Querying;

public static class FilterParser
{
    private const int MaxDepth = 8;
    private const int MaxConditions = 50;

    public static FilterNode? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            var context = new ParseContext();
            return ParseElement(document.RootElement, context, 0);
        }
        catch (JsonException ex)
        {
            throw new InvalidFilterException("Invalid JSON filter syntax.", ex);
        }
    }

    private static FilterNode ParseElement(JsonElement element, ParseContext context, int depth)
    {
        if (depth > MaxDepth)
            throw new InvalidFilterException($"Filter depth exceeds maximum of {MaxDepth}.");

        if (element.ValueKind != JsonValueKind.Array)
            throw new InvalidFilterException("Filter must be a JSON array.");

        var length = element.GetArrayLength();
        if (length == 0)
            throw new InvalidFilterException("Filter array cannot be empty.");

        // Check if it's a Condition: Exactly 3 elements, and the second element is NOT a logical operator "and"/"or"
        if (length == 3)
        {
            var opElement = element[1];
            if (opElement.ValueKind == JsonValueKind.String)
            {
                var opString = opElement.GetString();
                if (!string.Equals(opString, "and", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(opString, "or", StringComparison.OrdinalIgnoreCase))
                {
                    return ParseCondition(element, context);
                }
            }
        }

        // Otherwise, it must be a Group
        return ParseGroup(element, context, depth);
    }

    private static FilterCondition ParseCondition(JsonElement element, ParseContext context)
    {
        context.ConditionCount++;
        if (context.ConditionCount > MaxConditions)
            throw new InvalidFilterException($"Number of filter conditions exceeds maximum of {MaxConditions}.");

        var fieldElement = element[0];
        if (fieldElement.ValueKind != JsonValueKind.String)
            throw new InvalidFilterException("Left operand (field name) must be a string.");

        var fieldName = fieldElement.GetString()!;

        var opElement = element[1];
        if (opElement.ValueKind != JsonValueKind.String)
            throw new InvalidFilterException("Operator must be a string.");
        
        var op = opElement.GetString()!.ToLowerInvariant();
        var filterOp = ParseFilterOperator(op);

        var valueElement = element[2];
        object? value = ParseValue(valueElement);

        return new FilterCondition(fieldName, filterOp, value);
    }

    private static FilterGroup ParseGroup(JsonElement element, ParseContext context, int depth)
    {
        var length = element.GetArrayLength();
        if (length % 2 == 0)
            throw new InvalidFilterException("Filter group array must have an odd number of elements (nodes separated by logic operators).");

        LogicOperator? currentLogic = null;
        var nodes = new List<FilterNode>();

        for (int i = 0; i < length; i++)
        {
            var item = element[i];
            if (i % 2 == 0)
            {
                // Even indices must be nodes (conditions or groups)
                if (item.ValueKind != JsonValueKind.Array)
                    throw new InvalidFilterException("Expected a filter node (array) in group.");
                
                nodes.Add(ParseElement(item, context, depth + 1));
            }
            else
            {
                // Odd indices must be logic operators
                if (item.ValueKind != JsonValueKind.String)
                    throw new InvalidFilterException("Expected a logical operator ('and' or 'or') in group.");

                var logicStr = item.GetString();
                var logic = ParseLogicOperator(logicStr);

                if (currentLogic == null)
                {
                    currentLogic = logic;
                }
                else if (currentLogic != logic)
                {
                    throw new InvalidFilterException("Cannot mix 'AND' and 'OR' at the same level. Use nested arrays to specify precedence.");
                }
            }
        }

        return new FilterGroup(currentLogic ?? LogicOperator.And, nodes);
    }

    private static LogicOperator ParseLogicOperator(string? op)
    {
        if (string.Equals(op, "and", StringComparison.OrdinalIgnoreCase))
            return LogicOperator.And;
        if (string.Equals(op, "or", StringComparison.OrdinalIgnoreCase))
            return LogicOperator.Or;

        throw new InvalidFilterException($"Invalid logical operator '{op}'. Expected 'and' or 'or'.");
    }

    private static FilterOperator ParseFilterOperator(string op)
    {
        return op switch
        {
            "=" => FilterOperator.Equal,
            "!=" or "<>" => FilterOperator.NotEqual,
            ">" => FilterOperator.GreaterThan,
            "<" => FilterOperator.LessThan,
            ">=" => FilterOperator.GreaterThanOrEqual,
            "<=" => FilterOperator.LessThanOrEqual,
            "contains" => FilterOperator.Contains,
            "notcontains" => FilterOperator.NotContains,
            "startswith" => FilterOperator.StartsWith,
            "endswith" => FilterOperator.EndsWith,
            "between" => FilterOperator.Between,
            "in" => FilterOperator.In,
            "isnull" => FilterOperator.IsNull,
            "isnotnull" => FilterOperator.IsNotNull,
            _ => throw new InvalidFilterException($"Unsupported operator '{op}'.")
        };
    }

    private static object? ParseValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                // Return string representation of number to let the expression builder handle precise type conversion
                return element.GetRawText();
            case JsonValueKind.True:
                return "true";
            case JsonValueKind.False:
                return "false";
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ParseValue(item));
                }
                return list.ToArray();
            default:
                throw new InvalidFilterException($"Unsupported JSON value kind '{element.ValueKind}' in filter value.");
        }
    }

    private class ParseContext
    {
        public int ConditionCount { get; set; }
    }
}
