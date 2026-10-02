namespace Platform.Modules.IdentityAdmin.DependencyInjection;

using Platform.Modules.IdentityAdmin.Commands;
using Platform.Modules.IdentityAdmin.Queries;
using Microsoft.Extensions.DependencyInjection;
using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Application.Contracts.Queries;

public static class IdentityAdminDependencyInjection
{
    public static IServiceCollection AddIdentityAdminApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateUserCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<CreateUserCommand, CreateUserResponse>>(
            provider => provider.GetRequiredService<CreateUserCommandHandler>());

        services.AddScoped<UpdateUserCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<UpdateUserCommand>>(
            provider => provider.GetRequiredService<UpdateUserCommandHandler>());

        services.AddScoped<DeleteUserCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<DeleteUserCommand>>(
            provider => provider.GetRequiredService<DeleteUserCommandHandler>());

        services.AddScoped<CreateRoleCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<CreateRoleCommand, CreateRoleResponse>>(
            provider => provider.GetRequiredService<CreateRoleCommandHandler>());

        services.AddScoped<AssignPermissionToRoleCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<AssignPermissionToRoleCommand>>(
            provider => provider.GetRequiredService<AssignPermissionToRoleCommandHandler>());

        services.AddScoped<AssignPolicyToRoleCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<AssignPolicyToRoleCommand>>(
            provider => provider.GetRequiredService<AssignPolicyToRoleCommandHandler>());

        services.AddScoped<RemovePermissionFromRoleCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<RemovePermissionFromRoleCommand>>(
            provider => provider.GetRequiredService<RemovePermissionFromRoleCommandHandler>());

        services.AddScoped<RemovePolicyFromRoleCommandHandler>();
        services.AddScoped<IAsyncCommandHandler<RemovePolicyFromRoleCommand>>(
            provider => provider.GetRequiredService<RemovePolicyFromRoleCommandHandler>());

        services.AddScoped<GetUsersQueryHandler>();
        services.AddScoped<IAsyncQueryHandler<GetUsersQuery, IReadOnlyCollection<UserSummaryDto>>>(
            provider => provider.GetRequiredService<GetUsersQueryHandler>());

        services.AddScoped<GetRolesQueryHandler>();
        services.AddScoped<IAsyncQueryHandler<GetRolesQuery, IReadOnlyCollection<RoleSummaryDto>>>(
            provider => provider.GetRequiredService<GetRolesQueryHandler>());

        services.AddScoped<GetPermissionsQueryHandler>();
        services.AddScoped<IAsyncQueryHandler<GetPermissionsQuery, IReadOnlyCollection<string>>>(
            provider => provider.GetRequiredService<GetPermissionsQueryHandler>());

        services.AddScoped<GetPoliciesQueryHandler>();
        services.AddScoped<IAsyncQueryHandler<GetPoliciesQuery, IReadOnlyCollection<PolicySummaryDto>>>(
            provider => provider.GetRequiredService<GetPoliciesQueryHandler>());

        return services;
    }
}
