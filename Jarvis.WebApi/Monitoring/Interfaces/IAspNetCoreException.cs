using System.Diagnostics;

namespace Jarvis.WebApi.Monitoring.Interfaces;

public interface IAspNetCoreException
{
    void Enrich(Activity activity, Exception exception);
}