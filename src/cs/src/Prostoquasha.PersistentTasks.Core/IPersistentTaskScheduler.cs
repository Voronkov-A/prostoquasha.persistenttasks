using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Core;

public interface IPersistentTaskScheduler
{
    Task<PersistentTask<TParameters, TState, TResult>> ScheduleAsync<TParameters, TState, TResult>(
        PersistentTask<TParameters, TState, TResult> task,
        CancellationToken cancellationToken);

    Task WaitForCompletionAsync<TParameters, TState, TResult>(
        PersistentTask<TParameters, TState, TResult> task,
        CancellationToken cancellationToken);

    Task<PersistentTask<TParameters, TState, TResult>> GetOrDefaultAsync<TParameters, TState, TResult>(
        PersistentTaskId id,
        CancellationToken cancellationToken);

    Task<PersistentTaskCompletionSource<TResult>> RegisterAsync<TResult>(
        PersistentTaskCompletionSource<TResult> taskCompletionSource,
        CancellationToken cancellationToken);

    Task<PersistentTaskCompletionSource<TResult>> GetOrDefaultAsync<TResult>(
        PersistentTaskCompletionSourceId id,
        CancellationToken cancellationToken);

    Task<bool> TrySetResultAsync<TResult>(
        PersistentTaskCompletionSourceId id,
        TResult result,
        CancellationToken cancellationToken);

    Task<bool> TrySetErrorAsync(
        PersistentTaskCompletionSourceId id,
        ErrorInfo error,
        CancellationToken cancellationToken);

    Task<bool> TryCancelAsync(PersistentTaskCompletionSourceId id, CancellationToken cancellationToken);
}
