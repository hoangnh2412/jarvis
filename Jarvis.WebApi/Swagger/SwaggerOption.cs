namespace Jarvis.WebApi.Swagger;

public class SwaggerOption
{
    public bool Enable { get; set; }
    public string[] Versions { get; set; }
    public string Prefix { get; set; }
    public string[] SecuritySchemes { get; set; }
}