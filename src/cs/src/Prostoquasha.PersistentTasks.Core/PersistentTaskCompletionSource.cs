namespace Prostoquasha.PersistentTasks.Core;

public sealed class PersistentTaskCompletionSource<TResult>
{
    public PersistentTaskCompletionSource(PersistentTaskCompletionSourceId id, PersistentTaskId? parentId = null)
    {
        Id = id;
        ParentId = parentId;
    }

    public PersistentTaskCompletionSource(
        PersistentTaskCompletionSourceId id,
        TResult? result,
        PersistentTaskStatus status,
        PersistentTaskId? parentId,
        FailedAttempt? failedAttempt)
    {
        Id = id;
        Result = result;
        Status = status;
        ParentId = parentId;
        FailedAttempt = failedAttempt;
    }

    public PersistentTaskCompletionSourceId Id { get; }

    public TResult? Result { get; }

    public PersistentTaskStatus Status { get; } = PersistentTaskStatus.Waiting;

    public PersistentTaskId? ParentId { get; }

    public FailedAttempt? FailedAttempt { get; }
}
