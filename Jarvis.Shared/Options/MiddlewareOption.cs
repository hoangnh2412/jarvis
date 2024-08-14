namespace Jarvis.Shared.Options;

public class MiddlewareOption
{
    public List<string> AllowContentTypes { get; set; }

    public Dictionary<string, Dictionary<string, string>> Paths { get; set; }
}