namespace Jarvis.Domain.Common.V1;

public class Paging
{
    public int Start { get; set; } = 1;
    public int Length { get; set; } = 10;
    public string Column { get; set; }
    public bool IsAsc { get; set; } = true;
    public string Keyword { get; set; }

    public static Dictionary<string, string> ParseSort(string column, bool isAsc)
    {
        var sorts = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(column))
            return new Dictionary<string, string>();

        var splited = column.Split('.');
        foreach (var item in splited)
        {
            sorts.Add(item, isAsc ? "asc" : "desc");
        }

        return sorts;
    }
}