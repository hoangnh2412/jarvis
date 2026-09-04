using System.ComponentModel.DataAnnotations;
using Jarvis.DDD.Domain.Shared.ExceptionHandling;

namespace Jarvis.Modules.Notifications.Constants;

/// <summary>
/// Mã lỗi nghiệp vụ của module Notifications.
/// </summary>
public class NotificationErrorCode : IErrorCode
{
    [Display(Description = "Hệ thống chưa xác định được người dùng đang thao tác")]
    public const string UserRequired = "98201";

    [Display(Description = "Hệ thống chưa xác định được tenant đang làm việc")]
    public const string TenantRequired = "98202";
}
