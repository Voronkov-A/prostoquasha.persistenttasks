using System;

namespace Prostoquasha.PersistentTasks.Core;

public interface IPersistentTask
{
    PersistentTaskId Id { get; }

    //TParameters Parameters { get; }

    //TResult? Result { get; }

    PersistentTaskStatus Status { get; }

    //TState State { get; }

    PersistentTaskId? ParentId { get; }

    FailedAttempt? FailedAttempt { get; }

    bool IsCancellationRequested { get; }

    DateTimeOffset? ContinueAfter { get; }

    PersistentTaskStateType StateType { get; }
}
