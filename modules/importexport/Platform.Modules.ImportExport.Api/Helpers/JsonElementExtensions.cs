using System.Text.Json;

namespace Platform.Modules.ImportExport.API.Helpers;

public static class JsonElementExtensions
{
    public static object? ToClrValue(this JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number when value.TryGetInt64(out var integer) => integer,
        JsonValueKind.Number when value.TryGetDecimal(out var decimalValue) => decimalValue,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => value.GetRawText()
    };
}
