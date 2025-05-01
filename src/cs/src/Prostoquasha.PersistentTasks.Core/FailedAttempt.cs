namespace Prostoquasha.PersistentTasks.Core;

public sealed class FailedAttempt(ErrorInfo error, int errorRepeatCount)
{
    public ErrorInfo Error { get; } = error;

    public int ErrorRepeatCount { get; } = errorRepeatCount;
}
