namespace Sample.Multitenancy;

public static class MultitenancyEfTestServiceCollectionExtensions
{
    public static IServiceCollection AddMultitenancyEfTestHostedService(this IServiceCollection services)
    {
        services.AddScoped<MultitenancyEfJobRunner>();
        services.AddHostedService<MultitenancyEfTestHostedService>();
        return services;
    }
}
