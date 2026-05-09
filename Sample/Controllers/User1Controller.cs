using System.Diagnostics;
using Asp.Versioning;
using Jarvis.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Sample.Entities;
using Sample.Persistence;
using Sample.Telemetry;
using StackExchange.Redis;

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

    [ApiVersion("1.0")]
    [HttpGet]
    public async Task<IActionResult> GetV1(CancellationToken cancellationToken)
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
            var repo = unitOfWork.GetRepository<IRepository<Student>>();
            studentCount = await repo.CountAsync();
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

        return Ok(new
        {
            Data = "User API v1",
            StudentCount = studentCount,
            DbQuerySucceeded = dbOk,
            RedisHits = redisHits,
            RedisSucceeded = redisOk
        });
    }
}
