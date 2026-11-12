using Platform.DDD.Domain.Shared.Messaging;

namespace Platform.Modules.Account.Commands;

/// <summary>Đăng nhập tầng App: mã tenant, mã org, rồi user trên identity store của tenant.</summary>
public sealed record AppLoginCommand : ICommand
{
    public required string TenantCode { get; init; }

    public required string OrgCode { get; init; }

    public required string UserName { get; init; }

    public required string Password { get; init; }
}
