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
}
