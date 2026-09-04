using System.ComponentModel.DataAnnotations;
using Jarvis.DDD.Application.Contracts.DTOs;
using Jarvis.Modules.Notifications.Contracts;

namespace Module.Notifications.Models;

/// <summary>
/// Tham số query lấy danh sách thông báo của user hiện tại.
/// </summary>
public sealed class NotificationListQuery : IPagingDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int Size { get; set; } = 20;
    string? IPagingDto.Filter { get; set; }
    string? IPagingDto.Sort { get; set; }
    string? IPagingDto.Columns { get; set; }

    [EnumDataType(typeof(NotificationListFilter), ErrorMessage = "ReadStatus must be All, Unread, or Read.")]
    public NotificationListFilter ReadStatus { get; set; } = NotificationListFilter.All;
}