using Jarvis.Common.Enums;

namespace Jarvis.Mvc;

public class JsonOption
{
    public bool IgnoreNull { get; set; }

    public JsonNamingPolicy NamingPolicy { get; set; }
}