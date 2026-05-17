using System.Diagnostics;
using Asp.Versioning;
using Jarvis.Domain.Repositories;
using Jarvis.Domain.Shared.Enums;
using Jarvis.Domain.Shared.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Sample.Entities;
using Sample.ErrorCodes;
using Sample.Models;
using Sample.Persistence;
using Sample.Swagger;
using Sample.Telemetry;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;

namespace Sample.Controllers;

[ApiVersion(ApiVersions.Current)]
[ApiController]
[Route("api/v{version:apiVersion}/users")]
public class User1Controller(
    ILogger<User1Controller> logger,
    ISampleUnitOfWork unitOfWork,
    [FromKeyedServices("Default")] IConnectionMultiplexer redis) : ControllerBase
{
    /// <summary>Must match <c>AddSource("Sample")</c> in Jarvis OpenTelemetry defaults.</summary>
    private static readonly ActivitySource ActivitySource = new("Sample", "1.0.0");

    private const string RedisHitsKey = "sample:demo:users:v1:hits";

    /// <summary>Demo aggregate of PostgreSQL student count and Redis hit counter (v1).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>After <c>ApiResponseWrapperMiddleware</c>, JSON is <c>BaseResponse&lt;<see cref="UserV1GetData"/>&gt;</c> with this payload in <c>data</c>.</returns>
    [ApiVersion("1.0")]
    [HttpGet]
    [SwaggerResponseExample(200, typeof(UserV1GetResponseExampleProvider))]
    public async Task<ActionResult<UserV1GetData>> GetV1(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Sample.Users.V1.Get");
        activity?.SetTag("sample.feature", "manual-span");

        var sw = Stopwatch.StartNew();
        var studentCount = 0;
        var dbOk = false;
        long redisHits = 0;
        var redisOk = false;

        try
        {
            var repo = await unitOfWork.GetRepositoryAsync<IRepository<Student>>().ConfigureAwait(false);
            studentCount = await repo.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            dbOk = true;
            SampleUserV1Metrics.DbQueries.Add(1, new KeyValuePair<string, object?>("outcome", "success"));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Demo DB query failed (ensure PostgreSQL and migrations).");
            SampleUserV1Metrics.DbQueries.Add(1, new KeyValuePair<string, object?>("outcome", "error"));
        }

        try
        {
            var db = redis.GetDatabase();
            redisHits = await db.StringIncrementAsync(RedisHitsKey);
            await db.KeyExpireAsync(RedisHitsKey, TimeSpan.FromHours(24));
            redisOk = true;
            SampleUserV1Metrics.RedisOperations.Add(1, new KeyValuePair<string, object?>("op", "incr_hit"));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Demo Redis failed (ensure Redis at Cache:DistGroups:Redis:Default:Configuration).");
            SampleUserV1Metrics.RedisOperations.Add(1, new KeyValuePair<string, object?>("op", "error"));
        }
        finally
        {
            sw.Stop();
            SampleUserV1Metrics.HandlerDurationSeconds.Record(sw.Elapsed.TotalSeconds);
        }

        logger.LogInformation(
            "Demo Users v1: students={StudentCount}, redisHits={RedisHits}, dbOk={DbOk}, redisOk={RedisOk}",
            studentCount,
            redisHits,
            dbOk,
            redisOk);

        return Ok(new UserV1GetData
        {
            Data = "User API v1",
            StudentCount = studentCount,
            DbQuerySucceeded = dbOk,
            RedisHits = redisHits,
            RedisSucceeded = redisOk
        });
    }

    /// <summary>
    /// Demo HTTP 422: <see cref="BusinessException"/> + <see cref="SampleErrorCode"/>. Client đọc <b>code</b> trong JSON (ví dụ <c>Sample:99102</c>).
    /// </summary>
    [ApiVersion("1.0")]
    [HttpGet("demo-422/stale-write")]
    [SwaggerResponseExample(422, typeof(UserV1422ResponseExampleProvider))]
    public IActionResult Demo422StaleWrite()
    {
        throw new BusinessException(SampleErrorCode.DemoStaleWrite, systemMessage: "RowVersion: expected 3, actual 5");
    }

    /// <summary>
    /// Demo HTTP 422: mã <see cref="BaseErrorCode"/> — <c>code</c> trả về dạng <c>Sample:&lt;suffix&gt;</c> (ví dụ captcha).
    /// </summary>
    [ApiVersion("1.0")]
    [HttpGet("demo-422/captcha")]
    [SwaggerResponseExample(422, typeof(UserV1422ResponseExampleProvider))]
    public IActionResult Demo422CaptchaRequired()
    {
        throw new BusinessException(BaseErrorCode.CaptchaIsRequired, systemMessage: "Captcha token missing on request.");
    }

    /// <summary>
    /// Demo HTTP 422: vẫn có <c>code</c>; thêm <c>data</c> khi exception có <c>Content</c> (<c>BaseResponse&lt;object&gt;</c>).
    /// </summary>
    [ApiVersion("1.0")]
    [HttpGet("demo-422/with-content")]
    [SwaggerResponseExample(422, typeof(UserV1422WithDataResponseExampleProvider))]
    public IActionResult Demo422WithContent()
    {
        throw new BusinessException(
            SampleErrorCode.DemoCaptchaRequired,
            content: new { RequiredAction = "verify_captcha", ChallengeUrl = "/captcha/challenge-abc" },
            systemMessage: "Client should open ChallengeUrl and retry.");
    }

    /// <summary>
    /// Demo HTTP 422: hai nhánh <c>throw</c> với mã khác nhau — mỗi response vẫn có <c>code</c> rõ ràng (<c>branch=a</c> → <c>Sample:99103</c>, <c>b</c> → <c>Sample:99104</c>).
    /// </summary>
    /// <param name="branch"><c>a</c> hoặc <c>b</c> để đổi mã lỗi trong cùng endpoint.</param>
    [ApiVersion("1.0")]
    [HttpGet("demo-422/two-throw-sites")]
    [SwaggerResponseExample(422, typeof(UserV1422ResponseExampleProvider))]
    public IActionResult Demo422TwoThrowSites([FromQuery] string branch = "a")
    {
        if (string.Equals(branch, "a", StringComparison.OrdinalIgnoreCase))
            throw new BusinessException(SampleErrorCode.DemoLogicStep001, systemMessage: "Lần A: dừng tại bước kiểm tra nghiệp vụ đầu tiên.");

        if (string.Equals(branch, "b", StringComparison.OrdinalIgnoreCase))
            throw new BusinessException(SampleErrorCode.DemoLogicStep002, systemMessage: "Lần B: dừng tại bước kiểm tra nghiệp vụ thứ hai.");

        throw new BusinessException(
            SampleErrorCode.DemoCaptchaRequired,
            systemMessage: $"Chỉ dùng branch=a hoặc branch=b; nhận được '{branch}'.");
    }
}
