using Prostoquasha.PersistentTasks.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.InMemory;

public sealed class InMemoryPersistentTaskScheduler : IPersistentTaskScheduler
{
    public Task<PersistentTask<TParameters, TState, TResult>> ScheduleAsync<TParameters, TState, TResult>(
        PersistentTask<TParameters, TState, TResult> task,
        CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    public Task WaitForCompletionAsync<TParameters, TState, TResult>(
        PersistentTask<TParameters, TState, TResult> task,
        CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    private readonly struct PersistentTaskRecord
    {
        public required object Task { get; init; }

        public required TaskCompletionSource Completion { get; init; }
    }
}
