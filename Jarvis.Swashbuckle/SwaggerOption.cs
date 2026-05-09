namespace Jarvis.Swashbuckle;

public class SwaggerOption
{
    public bool Enable { get; set; }

    public string[] Versions { get; set; } = [];

    public string Prefix { get; set; } = string.Empty;

    public string[] SecuritySchemes { get; set; } = [];

    /// <summary>
    /// Header name for the API key scheme (OpenAPI). Default <c>X-API-KEY</c>.
    /// </summary>
    public string ApiKeyHeaderName { get; set; } = "X-API-KEY";

    /// <summary>
    /// When non-empty, <see cref="Filters.ProducesBaseResponseOperationFilter"/> only runs for operations whose relative path matches one of these prefixes (e.g. <c>/api</c>).
    /// When empty, the filter applies to all operations.
    /// </summary>
    public string[] ApiResponseDocumentationPathPrefixes { get; set; } = ["/api"];
}
