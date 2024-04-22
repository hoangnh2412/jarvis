using System.Diagnostics;
using Microsoft.AspNetCore.Http;

public interface IAspNetCoreHttpRequest
{
    void Enrich(Activity activity, HttpRequest httpRequest);
}