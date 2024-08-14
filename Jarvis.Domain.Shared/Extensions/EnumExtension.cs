using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jarvis.Domain.Shared.Extensions;

#nullable disable

/// <summary>
/// Provides extension functions for Enum
/// </summary>
public static partial class EnumExtension
{
    /// <summary>
    /// Return the value of Name property
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetName(this Enum value)
    {
        var items = value.GetType().GetMember(value.ToString());
        if (items.Length == 0)
            return null;

        var item = items.First().GetCustomAttribute<DisplayAttribute>();
        if (item == null)
            return items.First().GetCustomAttribute<DisplayNameAttribute>().DisplayName;
        else
            return item.GetName();
    }

    /// <summary>
    /// Return the value of ShortName property
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetShortName(this Enum value)
    {
        var items = value.GetType().GetMember(value.ToString());
        if (items.Length == 0)
            return null;

        var item = items.First().GetCustomAttribute<DisplayAttribute>();
        if (item == null)
            return null;
        return item.GetShortName();
    }

    /// <summary>
    /// Return the value of Description property
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum value)
    {
        var items = value.GetType().GetMember(value.ToString());
        if (items.Length == 0)
            return null;

        var item = items.First().GetCustomAttribute<DisplayAttribute>();
        if (item == null)
            return null;
        return item.GetDescription();
    }

    /// <summary>
    /// Return the value of GroupName property
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetGroupName(this Enum value)
    {
        var items = value.GetType().GetMember(value.ToString());
        if (items.Length == 0)
            return null;

        var item = items.First().GetCustomAttribute<DisplayAttribute>();
        if (item == null)
            return null;
        return item.GetGroupName();
    }
}