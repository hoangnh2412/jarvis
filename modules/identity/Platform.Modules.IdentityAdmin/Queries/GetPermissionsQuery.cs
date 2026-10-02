namespace Platform.Modules.IdentityAdmin.Queries;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Trả về tập permission mà actor hiện tại đang có (grantable set) — chưa có catalog permission
/// toàn cục trong hệ thống nên không trả "list all".
/// </summary>
public sealed record GetPermissionsQuery : IQuery;
