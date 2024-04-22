using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface IAspNetCoreEntricHttpResponse
{
    void Enrich(Activity activity, HttpResponse httpRequest);
}