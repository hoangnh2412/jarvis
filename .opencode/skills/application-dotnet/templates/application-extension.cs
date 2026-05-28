using Jarvis.Application;

namespace {Product}.Application;

public static class ApplicationLayerExtension
{
    public static IHostApplicationBuilder AddApplicationLayer(this IHostApplicationBuilder builder)
    {
        builder.AddCoreApplication();
        // builder.Services.AddScoped<IMyCommandHandler, MyCommandHandler>();
        return builder;
    }
}
