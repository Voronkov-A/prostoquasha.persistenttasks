namespace Prostoquasha.PersistentTasks.Core;

public enum PersistentTaskStatus
{
    Waiting,
    Executing,
    Succeeded,
    Failed,
    Canceled
}
