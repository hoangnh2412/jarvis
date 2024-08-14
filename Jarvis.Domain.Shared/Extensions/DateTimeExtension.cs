using System.Globalization;

namespace Jarvis.Domain.Shared.Extensions;

/// <summary>
/// Provides extension functions for DateTime
/// </summary>
public static partial class DateTimeExtension
{
    /// <summary>
    /// Return list of dates between two dates
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static IEnumerable<DateTime> DatesToDate(this DateTime from, DateTime to)
    {
        return Enumerable.Range(0, 1 + to.Subtract(from).Days)
              .Select(offset => from.AddDays(offset));
    }

    /// <summary>
    /// Return list of a dates in month between two dates
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <param name="day">Day in month, default = 1</param>
    /// <returns></returns>
    public static IEnumerable<DateTime> MonthsToDate(this DateTime from, DateTime to, int day = 1)
    {
        DateTime iterator;
        DateTime limit;

        if (to > from)
        {
            iterator = new DateTime(from.Year, from.Month, day);
            limit = to;
        }
        else
        {
            iterator = new DateTime(to.Year, to.Month, day);
            limit = from;
        }

        var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        while (iterator <= limit)
        {
            yield return new DateTime(iterator.Year, iterator.Month, day);
            iterator = iterator.AddMonths(1);
        }
    }

    /// <summary>
    /// Convert datetime to UnixTime second
    /// </summary>
    /// <param name="datetime"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static long ToUnixTimeSecond(this DateTime datetime, DateTimeKind kind = DateTimeKind.Utc)
    {
        return (long)(datetime - new DateTime(1970, 1, 1, 0, 0, 0, kind)).TotalSeconds;
    }

    /// <summary>
    /// Convert datetime to UnixTime millisecond
    /// </summary>
    /// <param name="datetime"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static long ToUnixTimeMillisecond(this DateTime datetime, DateTimeKind kind = DateTimeKind.Utc)
    {
        return (long)(datetime - new DateTime(1970, 1, 1, 0, 0, 0, kind)).TotalMilliseconds;
    }
}