namespace Prostoquasha.PersistentTasks.Core;

public enum PersistentTaskStatus
{
    Waiting,
    WaitingForAllChildren,
    WaitingForAnyChild,
    Executing,
    Succeeded,
    Failed,
    Canceled
}
