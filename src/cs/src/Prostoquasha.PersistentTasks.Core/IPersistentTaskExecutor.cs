using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Core;

public interface IPersistentTaskExecutor<TParameters, TState, TResult>
{
    Task<ExecutionResult<TState, TResult>> ExecuteAsync(
        PersistentTask<TParameters, TState, TResult> task,
        CancellationToken cancellationToken);
}
