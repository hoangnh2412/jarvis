namespace Sample.Multitenancy;

public static class MultitenancyEfTestHostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddMultitenancyEfTestHostedService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<MultitenancyEfJobRunner>();
        builder.Services.AddHostedService<MultitenancyEfTestHostedService>();
        return builder;
    }
}
