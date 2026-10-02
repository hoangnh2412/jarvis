using Microsoft.Extensions.Logging.Abstractions;
using Platform.Workflow.Idempotency;

namespace UnitTest.Workflow;

public sealed class WorkflowIdempotencyTests
{
    [Fact]
    public async Task MemoryStore_RejectsCompletionFromStaleLease()
    {
        var store = new MemoryWorkflowIdempotencyStore();
        var firstLease = await store.TryAcquireAsync("key", TimeSpan.FromMinutes(1));

        Assert.NotNull(firstLease);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.MarkCompletedAsync(
                firstLease! with { OwnerToken = "stale" },
                "{}"));
    }

    [Fact]
    public async Task Executor_DoesNotRunActionWhenAcquireFails()
    {
        var store = new FailingStore { FailAcquire = true };
        var executor = new WorkflowIdempotencyExecutor(
            store,
            NullLogger<WorkflowIdempotencyExecutor>.Instance);
        var actionCalled = false;

        await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(
            "key",
            () =>
            {
                actionCalled = true;
                return Task.FromResult(1);
            }));

        Assert.False(actionCalled);
    }

    [Fact]
    public async Task Executor_PropagatesCompletionFailureWithoutReleasingLease()
    {
        var store = new FailingStore { FailCompletion = true };
        var executor = new WorkflowIdempotencyExecutor(
            store,
            NullLogger<WorkflowIdempotencyExecutor>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(
            "key",
            () => Task.FromResult(1)));

        Assert.Equal(0, store.MarkFailedCount);
    }

    private sealed class FailingStore : IWorkflowIdempotencyStore
    {
        public bool FailAcquire { get; init; }
        public bool FailCompletion { get; init; }
        public int MarkFailedCount { get; private set; }

        public Task<WorkflowIdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult<WorkflowIdempotencyRecord?>(null);

        public Task<WorkflowIdempotencyLease?> TryAcquireAsync(string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            if (FailAcquire)
                throw new InvalidOperationException("storage unavailable");

            return Task.FromResult<WorkflowIdempotencyLease?>(
                new WorkflowIdempotencyLease(key, "owner", "stored"));
        }

        public Task MarkCompletedAsync(WorkflowIdempotencyLease lease, string? resultJson = null, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            if (FailCompletion)
                throw new InvalidOperationException("completion persistence failed");

            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(WorkflowIdempotencyLease lease, string? errorMessage = null, bool allowRetry = true, CancellationToken cancellationToken = default)
        {
            MarkFailedCount++;
            return Task.CompletedTask;
        }
    }
}
