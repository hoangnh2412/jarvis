using Platform.Common.Enums;

namespace Platform.Mvc;

public class JsonOption
{
    public bool IgnoreNull { get; set; }

    public JsonNamingPolicy NamingPolicy { get; set; }
}