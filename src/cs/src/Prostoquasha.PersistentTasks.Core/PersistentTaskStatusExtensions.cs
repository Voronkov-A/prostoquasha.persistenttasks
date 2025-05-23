namespace Prostoquasha.PersistentTasks.Core;

public static class PersistentTaskStatusExtensions
{
    public static bool IsCompleted(this PersistentTaskStatus status)
    {
        return status is PersistentTaskStatus.Succeeded or PersistentTaskStatus.Failed or PersistentTaskStatus.Canceled;
    }
}
