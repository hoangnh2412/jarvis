using System.ComponentModel.DataAnnotations;

namespace Jarvis.Infrastructure.DistributedEvent.EventBridge;

public enum EventAction
{
    [Display(Name = "create")]
    Create = 1,

    [Display(Name = "update")]
    Update = 2,

    [Display(Name = "delete")]
    Delete = 3,

    [Display(Name = "upsert")]
    Upsert = 4
}