using Jarvis.Domain.Shared.RequestResponse;
using Swashbuckle.AspNetCore.Filters;

namespace Sample.Swagger;

/// <summary>
/// Ví dụ thân lỗi HTTP 422 sau middleware: luôn có <c>code</c> để client map mã nghiệp vụ (<c>IErrorCode</c>).
/// Dùng chung cho các action demo <c>BusinessException</c> trong <see cref="Controllers.User1Controller"/>.
/// </summary>
public class UserV1422ResponseExampleProvider : IExamplesProvider<BaseResponse>
{
    public BaseResponse GetExamples() =>
        new()
        {
            TraceId = "4bf92f3577b34da6a3ce929d0e0e4736",
            SpanId = "00f067aa0ba902b7",
            TraceParent = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
            Timestamp = new DateTime(2026, 5, 10, 14, 30, 0, DateTimeKind.Utc),
            Code = "Sample:99102",
            Error = new BaseResponseError(
                message: "The data you edited has been changed by another user. Please refresh and try again.",
                systemMessage: "RowVersion: expected 3, actual 5")
        };
}
