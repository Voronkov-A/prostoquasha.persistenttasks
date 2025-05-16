using Prostoquasha.PersistentTasks.Core;
using Prostoquasha.PersistentTasks.Sample.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample.Commands;

internal sealed class StartSynchronizationCommandHandler(IPersistentTaskScheduler persistentTaskScheduler)
{
    private readonly IPersistentTaskScheduler _persistentTaskScheduler = persistentTaskScheduler;

    public async Task HandleAsync(StartSynchronizationCommand command, CancellationToken cancellationToken)
    {
        var task = SynchronizeRootDirectoryTask.Create(
            new SynchronizeRootDirectoryTask.Params
            {
                SourceDirectoryUrl = command.SourceDirectoryUrl,
                DestinationDirectoryUrl = command.DestinationDirectoryUrl
            });
        await _persistentTaskScheduler.ScheduleAsync(task, cancellationToken);
    }
}
