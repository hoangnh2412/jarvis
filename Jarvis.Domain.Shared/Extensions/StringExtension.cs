using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Jarvis.Domain.Shared.Constants;

namespace Jarvis.Domain.Shared.Extensions;

#nullable disable

public static partial class StringExtension
{
    #region Random
    public static readonly char[] UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    public static readonly char[] LowerChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    public static readonly char[] NumberChars = "0123456789".ToCharArray();
    public static readonly char[] SpecialChars = "!@#$%^*&".ToCharArray();

    public static string GenerateRandom(int length, bool isIncludeUpper = true, bool isIncludeLower = true,
        bool isIncludeNumber = true, bool isIncludeSpecial = false)
    {
        var chars = new List<char>();

        if (isIncludeUpper)
        {
            chars.AddRange(UpperChars);
        }

        if (isIncludeLower)
        {
            chars.AddRange(LowerChars);
        }

        if (isIncludeNumber)
        {
            chars.AddRange(NumberChars);
        }

        if (isIncludeSpecial)
        {
            chars.AddRange(SpecialChars);
        }

        return GenerateRandom(length, chars.ToArray());
    }

    public static string GenerateRandom(int length, params char[] chars)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), $"{length} cannot be less than zero.");
        }

        if (chars?.Any() != true)
        {
            throw new ArgumentOutOfRangeException(nameof(chars), $"{nameof(chars)} cannot be empty.");
        }

        chars = chars.Distinct().ToArray();

        const int maxLength = 256;

        if (maxLength < chars.Length)
        {
            throw new ArgumentException($"{nameof(chars)} may contain more than {maxLength} chars.", nameof(chars));
        }

        var outOfRangeStart = maxLength - (maxLength % chars.Length);

        using (var rng = RandomNumberGenerator.Create())
        {
            var sb = new StringBuilder();

            var buffer = new byte[128];

            while (sb.Length < length)
            {
                rng.GetBytes(buffer);

                for (var i = 0; i < buffer.Length && sb.Length < length; ++i)
                {
                    if (outOfRangeStart <= buffer[i])
                    {
                        continue;
                    }

                    sb.Append(chars[buffer[i] % chars.Length]);
                }
            }

            return sb.ToString();
        }
    }
    #endregion

    #region Norm
    /// <summary>
    ///     Normalize: UPPER case with remove all diacritic (accents) and convert edge case to
    ///                normal char in string
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         If value is is <c> null </c> or <c> whitespace </c> will return <c> empty string </c>
    ///     </para>
    ///     <para> See more: https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1308-normalize-strings-to-uppercase </para>
    /// </remarks>
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        value = value.Trim();

        // Convert Edge case
        value = string.Join(string.Empty, value.Select(ConvertEdgeCases));

        var normalizedString = RemoveAccents(value);

        return normalizedString.ToUpperInvariant();
    }

    public static string ConvertEdgeCases(char c)
    {
        if ("àåáâäãåąā".Contains(c))
            return "a";
        if ("èéêěëę".Contains(c))
            return "e";
        if ("ìíîïı".Contains(c))
            return "i";
        if ("òóôõöøőð".Contains(c))
            return "o";
        if ("ùúûüŭů".Contains(c))
            return "u";
        if ("çćčĉ".Contains(c))
            return "c";
        if ("żźž".Contains(c))
            return "z";
        if ("śşšŝ".Contains(c))
            return "c";
        if ("ñń".Contains(c))
            return "n";
        if ("ýÿ".Contains(c))
            return "y";
        if ("ğĝ".Contains(c))
            return "g";
        if ("ŕř".Contains(c))
            return "r";
        if ("ĺľł".Contains(c))
            return "l";
        if ("úů".Contains(c))
            return "u";
        if ("đď".Contains(c))
            return "d";
        if ('ť' == c)
            return "t";
        if ('ž' == c)
            return "z";
        if ('ß' == c)
            return "ss";
        if ('Þ' == c)
            return "th";
        if ('ĥ' == c)
            return "h";
        if ('ĵ' == c)
            return "j";

        return c.ToString();
    }

    /// <summary>
    ///     Remove all diacritics (accents) in string 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks> Already handle edge case <see cref="ConvertEdgeCases" /> </remarks>
    public static string RemoveAccents(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var normalizedString = value.Normalize(NormalizationForm.FormD);

        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

            if (unicodeCategory == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var edgeCases = ConvertEdgeCases(c);

            stringBuilder.Append(edgeCases);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
    #endregion

    #region Email
    /// <summary>
    /// Validate 1 email
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static bool IsEmail(this string source)
    {
        return Regex.IsMatch(source.Trim(), RegexValidate.EmailStandard, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
    }

    /// <summary>
    /// Validate nhiều email
    /// </summary>
    /// <param name="source">Danh sách email dc ngăn cách bởi delimiter</param>
    /// <param name="delimiter">Dấu ngăn cách giữa các email</param>
    /// <returns></returns>
    public static bool IsEmails(this string source, char delimiter)
    {
        var splited = source.Split(delimiter);

        foreach (var item in splited)
        {
            if (!IsEmail(item))
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    public static DateTime? ToDateTime(this string str, string format)
    {
        try
        {
            return DateTime.ParseExact(str, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert string to Enum
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ToEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    /// <summary>
    /// Try convert string to Enum. If error then return default value
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T TryToEnum<T>(string value)
    {
        try
        {
            return ToEnum<T>(value);
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    public static bool IsHtml(this string text)
    {
        Regex tagRegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");

        return tagRegex.IsMatch(text);
    }


    public static string ToCamelCase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && str.Length > 1)
        {
            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
        return str;
    }

    public static T ToGenericType<T>(this string value)
    {
        try
        {
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(value);
            // return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (System.Exception)
        {
            return default(T);
        }
    }
}