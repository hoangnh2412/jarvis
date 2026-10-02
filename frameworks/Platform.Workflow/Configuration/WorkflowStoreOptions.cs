namespace Platform.Workflow.Configuration;

public sealed class WorkflowStoreOptions
{
    public TimeSpan NodeInfoTtl { get; init; } = TimeSpan.FromDays(30);

    public TimeSpan IdempotencyTtl { get; init; } = TimeSpan.FromHours(24);

    public string KeyPrefix { get; init; } = "elsaworkflow";
}
