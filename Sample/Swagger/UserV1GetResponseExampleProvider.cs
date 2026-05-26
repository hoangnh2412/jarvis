using Jarvis.Domain.Shared.RequestResponse;
using Sample.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Sample.Swagger;

/// <summary>
/// Sample value for Swagger UI (200) on <c>User1Controller.GetV1</c>; illustrates <c>BaseResponse&lt;UserV1GetData&gt;</c> after the response wrapper.
/// </summary>
public class UserV1GetResponseExampleProvider : IExamplesProvider<BaseResponse<UserV1GetData>>
{
    public BaseResponse<UserV1GetData> GetExamples() =>
        new()
        {
            TraceId = "4bf92f3577b34da6a3ce929d0e0e4736",
            SpanId = "00f067aa0ba902b7",
            TraceParent = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
            Timestamp = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Utc),
            Code = "Sample:00000",
            Error = null,
            Data = new UserV1GetData
            {
                Data = "User API v1",
                StudentCount = 42,
                DbQuerySucceeded = true,
                RedisHits = 128,
                RedisSucceeded = true
            }
        };
}
