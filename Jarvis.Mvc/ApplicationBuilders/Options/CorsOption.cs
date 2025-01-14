namespace Jarvis.Mvc.ApplicationBuilders.Options;

public class CorsOption
{
    public bool AllowAnyOrigin { get; set; }
    public string[] AllowedOrigins { get; set; } = [];
    public bool AllowedAllHeaders { get; set; }
    public string[] AllowedHeaders { get; set; } = [];
    public bool AllowedAllMethods { get; set; }
    public string[] AllowedMethods { get; set; } = [];
}