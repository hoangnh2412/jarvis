namespace Jarvis.Mvc;

public class CorsOption
{
    public bool AllowedAllOrigins { get; set; }
    public string[] AllowedOrigins { get; set; } = [];

    public bool AllowedAllHeaders { get; set; }
    public string[] AllowedHeaders { get; set; } = [];

    public bool AllowedAllMethods { get; set; }
    public string[] AllowedMethods { get; set; } = [];
}