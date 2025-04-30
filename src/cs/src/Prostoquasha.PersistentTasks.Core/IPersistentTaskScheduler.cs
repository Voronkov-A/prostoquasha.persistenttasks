using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Core;

public interface IPersistentTaskScheduler
{
    Task<PersistentTask<TParameters, TState>> ScheduleAsync<TParameters, TState>(
        PersistentTask<TParameters, TState> task,
        CancellationToken cancellationToken);
}
