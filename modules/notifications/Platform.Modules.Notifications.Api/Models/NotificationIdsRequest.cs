using System.ComponentModel.DataAnnotations;

namespace Module.Notifications.Models;

/// <summary>
/// Request để đánh dấu thông báo đã đọc/chưa đọc
/// </summary>
public sealed class NotificationIdsRequest : IValidatableObject
{
    [Required]
    [MinLength(1)]
    [MaxLength(500)]
    public required Guid[] NotificationIds { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NotificationIds.Any(id => id == Guid.Empty))
        {
            yield return new ValidationResult(
                "Notification IDs must not contain Guid.Empty.",
                [nameof(NotificationIds)]);
        }
    }
}
