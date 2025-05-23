using System;

namespace Prostoquasha.PersistentTasks.Core;

public sealed class PersistentTask<TParameters, TState, TResult>
{
    public PersistentTask(
        PersistentTaskId id,
        TParameters parameters,
        TState state,
        PersistentTaskId? parentId = null,
        PersistentTaskStateType stateType = PersistentTaskStateType.Task)
    {
        Id = id;
        Parameters = parameters;
        State = state;
        ParentId = parentId;
        StateType = stateType;
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
        DateTimeOffset? continueAfter,
        PersistentTaskStateType stateType)
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
        StateType = stateType;
    }

    public PersistentTaskId Id { get; }

    public TParameters Parameters { get; }

    public TResult? Result { get; }

    public PersistentTaskStatus Status { get; } = PersistentTaskStatus.Waiting;

    public TState State { get; }

    public PersistentTaskId? ParentId { get; }

    public FailedAttempt? FailedAttempt { get; }

    public bool IsCancellationRequested { get; }

    public DateTimeOffset? ContinueAfter { get; }

    public PersistentTaskStateType StateType { get; } = PersistentTaskStateType.Task;
}
