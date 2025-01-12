using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Jarvis.Domain.Shared.Utility;

namespace Jarvis.Domain.Shared.ExceptionHandling;

public static class ErrorCodeHelper
{
    private static Dictionary<string, string>? _errorCodes = null;
    private static readonly object _lock = new();

    public static string GetMessage(string code)
    {
        var errorCodes = GetAllErrorCodes();
        if (errorCodes.TryGetValue(code, out string? value))
            return value;

        return string.Empty;
    }

    private static Dictionary<string, string> GetAllErrorCodes()
    {
        if (_errorCodes != null)
            return _errorCodes;

        lock (_lock)
        {
            if (_errorCodes != null)
                return _errorCodes;

            var items = new Dictionary<string, string>();
            var types = TypeHelper.GetAllClassSubTypes<IErrorCode>();
            foreach (var type in types)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(f => f.IsLiteral && !f.IsInitOnly);
                if (fields == null)
                    continue;

                foreach (var field in fields)
                {
                    var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();

                    if (displayAttribute == null)
                        continue;

                    if (string.IsNullOrEmpty(displayAttribute.Description))
                        continue;

                    var value = field.GetValue(null);
                    if (value == null)
                        continue;

                    items.Add(value.ToString()!, displayAttribute.Description);
                }
            }
            _errorCodes = items;
        }

        return _errorCodes;
    }
}