using System.ComponentModel.DataAnnotations;
using Jarvis.Domain.Shared.ExceptionHandling;

namespace Sample.ErrorCodes;

/// <summary>
/// Mã lỗi nghiệp vụ demo cho Sample (định dạng số để tương thích <see cref="BusinessException"/> / <c>HResult</c>).
/// Xuất hiện trong catalog <see cref="Jarvis.Domain.Shared.ExceptionHandling.ErrorCodeHelper"/> sau khi assembly được load.
/// </summary>
public class SampleErrorCode : IErrorCode
{
    [Display(Description = "Demo: yêu cầu xác thực captcha trước khi thực hiện thao tác.")]
    public const string DemoCaptchaRequired = "99101";

    [Display(Description = "Demo: bản ghi đã bị thay đổi bởi request khác (stale write / concurrent update).")]
    public const string DemoStaleWrite = "99102";

    /// <summary>Mã khi nhánh logic A ném lỗi (ví dụ bước kiểm tra thứ nhất).</summary>
    [Display(Description = "Demo nhánh A: lỗi nghiệp vụ tại bước 001 trong luồng xử lý.")]
    public const string DemoLogicStep001 = "99103";

    /// <summary>Mã khi nhánh logic B ném lỗi (ví dụ bước kiểm tra thứ hai).</summary>
    [Display(Description = "Demo nhánh B: lỗi nghiệp vụ tại bước 002 trong luồng xử lý.")]
    public const string DemoLogicStep002 = "99104";
}
