namespace Platform.Modules.Account.DependencyInjection;

using Platform.Modules.Account.Abstractions;
using Platform.Modules.Account.Commands;
using Platform.Modules.Account.Queries;
using Platform.Modules.Account.Services;
using Microsoft.Extensions.DependencyInjection;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Application.Contracts.Queries;

public static class AccountDependencyInjection
{
    public static IServiceCollection AddAccountApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<LoginCommand, LoginResponse>>(
            provider => provider.GetRequiredService<LoginCommandHandler>());

        services.AddScoped<LogoutCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<LogoutCommand>>(
            provider => provider.GetRequiredService<LogoutCommandHandler>());

        services.AddScoped<ChangePasswordCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<ChangePasswordCommand>>(
            provider => provider.GetRequiredService<ChangePasswordCommandHandler>());

        services.AddScoped<ForgotPasswordCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<ForgotPasswordCommand>>(
            provider => provider.GetRequiredService<ForgotPasswordCommandHandler>());

        services.AddScoped<ResetPasswordCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<ResetPasswordCommand>>(
            provider => provider.GetRequiredService<ResetPasswordCommandHandler>());

        services.AddScoped<UpdateProfileCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<UpdateProfileCommand>>(
            provider => provider.GetRequiredService<UpdateProfileCommandHandler>());

        services.AddScoped<GetMyProfileQueryHandler>();
        services.AddScoped<IAsyncQueryHandler<GetMyProfileQuery, MyProfileResponse>>(
            provider => provider.GetRequiredService<GetMyProfileQueryHandler>());

        return services;
    }
}
