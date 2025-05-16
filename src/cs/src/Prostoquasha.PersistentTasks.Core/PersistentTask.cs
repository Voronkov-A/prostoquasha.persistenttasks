using System;

namespace Prostoquasha.PersistentTasks.Core;

public sealed class PersistentTask<TParameters, TState, TResult>
{
    public PersistentTask(PersistentTaskId id, TParameters parameters, TState state, PersistentTaskId? parentId = null)
    {
        Id = id;
        Parameters = parameters;
        State = state;
        ParentId = parentId;
    }

    public PersistentTask(
        PersistentTaskId id,
        TParameters parameters,
        TResult? result,
        PersistentTaskStatus status,
        TState state,
        PersistentTaskId? parentId,
        FailedAttempt? failedAttempt,
        bool isCancellationRequested,
        DateTimeOffset? continueAfter)
    {
        Id = id;
        Parameters = parameters;
        Result = result;
        Status = status;
        State = state;
        ParentId = parentId;
        FailedAttempt = failedAttempt;
        IsCancellationRequested = isCancellationRequested;
        ContinueAfter = continueAfter;
    }

    public PersistentTaskId Id { get; }

    public TParameters Parameters { get; }

    public TResult? Result { get; }

    public PersistentTaskStatus Status { get; }

    public TState State { get; }

    public PersistentTaskId? ParentId { get; }

    public FailedAttempt? FailedAttempt { get; }

    public bool IsCancellationRequested { get; }

    public DateTimeOffset? ContinueAfter { get; }
}
