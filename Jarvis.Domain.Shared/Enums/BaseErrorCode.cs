using System.ComponentModel.DataAnnotations;

namespace Jarvis.Domain.Shared.Enums;

/// <summary>
/// The class defining the default error codes
/// </summary>
public class BaseErrorCode
{
    [Display(Description = "OK")]
    public const string OK = "00000";

    [Display(Description = "Something went wrong")]
    public const string Error = "99999";

    [Display(Description = "Reached request limit.")]
    public const string RateLimited = "99001";

    [Display(Description = "Captcha is required")]
    public const string CaptchaIsRequired = "99002";

    [Display(Description = "The data you edited has been changed by another user. Please refresh and try again.")]
    public const string ConcurrentUpdateOccurred = "99003";
}
