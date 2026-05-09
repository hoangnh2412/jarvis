namespace Sample.Models;

/// <summary>
/// Body payload for <c>GET /api/v{version}/users</c> when <c>version</c> is <c>1.0</c>.
/// The HTTP pipeline wraps this in <see cref="Jarvis.Domain.Shared.RequestResponse.BaseResponse{T}"/> (see <c>ApiResponseWrapperMiddleware</c>).
/// </summary>
public class UserV1GetData
{
    /// <summary>Short label identifying this demo API slice.</summary>
    /// <example>User API v1</example>
    public string Data { get; set; } = string.Empty;

    /// <summary>Count of student rows from PostgreSQL in the demo.</summary>
    /// <example>42</example>
    public int StudentCount { get; set; }

    /// <summary>Whether the demo database query completed without error.</summary>
    /// <example>true</example>
    public bool DbQuerySucceeded { get; set; }

    /// <summary>Hit counter stored in Redis for this endpoint.</summary>
    /// <example>128</example>
    public long RedisHits { get; set; }

    /// <summary>Whether the Redis increment completed without error.</summary>
    /// <example>true</example>
    public bool RedisSucceeded { get; set; }
}
