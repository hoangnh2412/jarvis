using System.Globalization;
using System.Text;

namespace Platform.OrgStructure;

public static class OrgStructureLimits
{
    public const int CodeMaxLength = 64;

    public const int NameMaxLength = 256;

    public const int UnitTypeMaxLength = 64;

    public const int PathMaxLength = 1024;

    public const int TaxIdMaxLength = 32;

    public const string SimpleCompanyTemplate = "simple-company";

    public const string CorporationTemplate = "corporation";

    public const string HoldingGroupTemplate = "holding-group";

    public static string NormalizeCode(string value)
    {
        var slug = RemoveDiacritics(value.Trim().ToLowerInvariant());
        var builder = new StringBuilder(slug.Length);
        var pendingHyphen = false;
        foreach (var character in slug)
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                if (pendingHyphen && builder.Length > 0)
                    builder.Append('-');
                pendingHyphen = false;
                builder.Append(character);
                continue;
            }

            pendingHyphen = true;
        }

        return builder.ToString();
    }

    private static string RemoveDiacritics(string value)
    {
        var formD = value.Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(formD.Length);
        foreach (var character in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
