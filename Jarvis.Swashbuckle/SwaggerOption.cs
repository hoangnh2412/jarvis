namespace Jarvis.Swashbuckle;

public class SwaggerOption
{
    public bool Enable { get; set; }
    public string[] Versions { get; set; } = [];
    public string Prefix { get; set; } = string.Empty;
    public string[] SecuritySchemes { get; set; } = [];
}