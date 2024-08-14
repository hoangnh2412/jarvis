using System.ComponentModel.DataAnnotations;

namespace Jarvis.Shared.Enums;

public enum EventBusName
{
    [Display(Name = "User")]
    User = 1,

    [Display(Name = "Pro")]
    Pro = 2,

    [Display(Name = "Diagnostic")]
    Diagnostic = 3,

    [Display(Name = "DiagnosticDetail")]
    DiagnosticDetail = 4
}