using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jarvis.Shared.Utility;

public static class EnumHelper
{
    /// <summary>
    /// Convert items of Enum to key-value
    /// Key = HashCode
    /// Value = Name property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Dictionary<int, string> ToDictionaryByHashCode<T>()
    {
        var items = Enum.GetValues(typeof(T)).Cast<T>();
        var results = new Dictionary<int, string>();
        foreach (var item in items)
        {
            var key = item.GetHashCode();
            var members = item.GetType().GetMember(item.ToString());
            if (members.Length == 0)
                throw new ArgumentNullException("Display Attribute is null");

            string value;
            var member = members.First().GetCustomAttribute<DisplayNameAttribute>();
            if (member == null)
                value = members.First().GetCustomAttribute<DisplayAttribute>().GetName();
            else
                value = members.First().GetCustomAttribute<DisplayNameAttribute>().DisplayName;

            results.Add(key, value);
        }
        return results;
    }

    /// <summary>
    /// Convert items of Enum to key-value
    /// Key = nameof(Enum)
    /// Value = Name property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Dictionary<string, string> ToDictionary<T>()
    {
        var items = Enum.GetValues(typeof(T)).Cast<T>();
        var results = new Dictionary<string, string>();
        foreach (var item in items)
        {
            var key = item.ToString();
            var members = item.GetType().GetMember(key);
            if (members.Length == 0)
                throw new ArgumentNullException("Display Attribute is null");

            string value;
            var member = members.First().GetCustomAttribute<DisplayNameAttribute>();
            if (member == null)
                value = members.First().GetCustomAttribute<DisplayAttribute>().GetName();
            else
                value = members.First().GetCustomAttribute<DisplayNameAttribute>().DisplayName;

            results.Add(key, value);
        }
        return results;
    }

}