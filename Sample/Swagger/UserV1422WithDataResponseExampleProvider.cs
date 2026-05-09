using Jarvis.Domain.Shared.RequestResponse;
using Swashbuckle.AspNetCore.Filters;

namespace Sample.Swagger;

/// <summary>Ví dụ 422 khi exception có <c>Content</c> — thân là <c>BaseResponse&lt;object&gt;</c> với <c>data</c> và vẫn có <c>code</c>.</summary>
public class UserV1422WithDataResponseExampleProvider : IExamplesProvider<BaseResponse<object>>
{
    public BaseResponse<object> GetExamples() =>
        new()
        {
            TraceId = "4bf92f3577b34da6a3ce929d0e0e4736",
            SpanId = "00f067aa0ba902b7",
            TraceParent = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
            Timestamp = new DateTime(2026, 5, 10, 14, 35, 0, DateTimeKind.Utc),
            Code = "Sample:99101",
            Error = new BaseResponseError(
                message: "Captcha is required",
                systemMessage: "Client should open ChallengeUrl and retry."),
            Data = new Dictionary<string, object>
            {
                ["RequiredAction"] = "verify_captcha",
                ["ChallengeUrl"] = "/captcha/challenge-abc"
            }
        };
}
