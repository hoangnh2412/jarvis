using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Domain.Constants;

/// <summary>JSON mapping parse tokens shared by API host and DLL consumers.</summary>
public static class MappingJsonConstants
{
    public const string AutoMapAll = "*";
    public const string EmptyArray = "[]";

    /// <summary>JSON property names (camelCase of <see cref="ColumnMapping"/> members via nameof).</summary>
    public static class Properties
    {
        public static readonly string ColumnName = ToCamel(nameof(ColumnMapping.ColumnName));
        public static readonly string FieldName = ToCamel(nameof(ColumnMapping.FieldName));
        public static readonly string Required = ToCamel(nameof(ColumnMapping.Required));
        public static readonly string Unique = ToCamel(nameof(ColumnMapping.Unique));
        public static readonly string Regex = ToCamel(nameof(ColumnMapping.Regex));
        public static readonly string MinLength = ToCamel(nameof(ColumnMapping.MinLength));
        public static readonly string MaxLength = ToCamel(nameof(ColumnMapping.MaxLength));
        public static readonly string Min = ToCamel(nameof(ColumnMapping.Min));
        public static readonly string Max = ToCamel(nameof(ColumnMapping.Max));

        private static string ToCamel(string name) =>
            string.IsNullOrEmpty(name)
                ? name
                : char.ToLowerInvariant(name[0]) + name[1..];
    }

    public const char JsonArrayStart = '[';
    public const char JsonObjectStart = '{';

    public const int PreviewMaxLength = 120;
    public const string PreviewEllipsis = "...";
    public const string PreviewSeparator = " | ";

    public const char Bom = '\uFEFF';
    public const char LeftDoubleQuote = '\u201C';
    public const char RightDoubleQuote = '\u201D';
    public const char LeftSingleQuote = '\u2018';
    public const char RightSingleQuote = '\u2019';
    public const char AsciiDoubleQuote = '"';
    public const char AsciiSingleQuote = '\'';
}