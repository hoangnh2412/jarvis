namespace Jarvis.Mvc.ApplicationBuilders.Options;

public class CorsOption
{
    public bool AllowAnyOrigin { get; set; } = true;
    public string[] AllowedOrigins { get; set; } = [];
    public bool AllowedAllHeaders { get; set; } = true;
    public string[] AllowedHeaders { get; set; } = [];
    public bool AllowedAllMethods { get; set; } = true;
    public string[] AllowedMethods { get; set; } = [];
    public bool AllowCredentials { get; set; } = true;
}