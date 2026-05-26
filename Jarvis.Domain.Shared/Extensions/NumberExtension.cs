namespace Jarvis.Domain.Shared.Extensions;

#nullable disable

/// <summary>
/// Provides extension functions for Number
/// </summary>
public static partial class NumberExtension
{
    /// <summary>
    /// Convert UnixTime second to DateTime
    /// </summary>
    /// <param name="unixtime"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static DateTime FromUnixTimeSecond(this long unixtime, DateTimeKind kind = DateTimeKind.Utc)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0, kind).AddSeconds(unixtime);
    }

    /// <summary>
    /// Convert UnixTime millisecond to DateTime
    /// </summary>
    /// <param name="unixtime"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static DateTime FromUnixTimeMillisecond(this long unixtime, DateTimeKind kind = DateTimeKind.Utc)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0, kind).AddMilliseconds(unixtime);
    }

    /// <summary>
    /// Convert double to day, hour, min, sec
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="isConsiderDay"></param>
    /// <returns></returns>
    public static Dictionary<string, int> ToPartsOfTime(this double duration, bool isConsiderDay)
    {
        Dictionary<string, int> durationParts = new Dictionary<string, int>();
        long numSecPerDay = 60 * 60 * 24;
        int numSecPerHour = 60 * 60;
        int numSecPerMin = 60;

        int numDay = 0;
        int numHour = 0;
        int numMin = 0;
        int numSec = 0;
        double remainDurationAfterGetNumDay = 0;
        double remainDurationAfterGetNumHour = 0;
        double remainDurationAfterGetNumMin = 0;

        if (isConsiderDay)
            numDay = Convert.ToInt32(Math.Floor((decimal)duration / numSecPerDay));

        durationParts.Add("NUM_DAY", numDay);

        remainDurationAfterGetNumDay = (double)duration - numDay * numSecPerDay;
        numHour = Convert.ToInt32(Math.Floor((decimal)remainDurationAfterGetNumDay / numSecPerHour));
        durationParts.Add("NUM_HOUR", numHour);

        remainDurationAfterGetNumHour = remainDurationAfterGetNumDay - numHour * numSecPerHour;
        numMin = Convert.ToInt32(Math.Floor((decimal)remainDurationAfterGetNumHour / numSecPerMin));
        durationParts.Add("NUM_MIN", numMin);

        remainDurationAfterGetNumMin = remainDurationAfterGetNumHour - numMin * numSecPerMin;
        numSec = Convert.ToInt32(Math.Floor((decimal)remainDurationAfterGetNumMin));
        durationParts.Add("NUM_SEC", numSec);

        return durationParts;
    }

    /// <summary>
    /// Convert number to Enum
    /// </summary>
    /// <param name="number"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ToEnum<T>(this int number)
    {
        return (T)Enum.Parse(typeof(T), number.ToString(), true);
    }

    /// <summary>
    /// Try convert number to Enum. If error then return default value
    /// </summary>
    /// <param name="number"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T TryToEnum<T>(this int number)
    {
        try
        {
            return ToEnum<T>(number);
        }
        catch (Exception)
        {
            return default(T);
        }
    }
}