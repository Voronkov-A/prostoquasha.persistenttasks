using Prostoquasha.PersistentTasks.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample;

internal sealed class PersistentTaskScheduler : IPersistentTaskScheduler
{
    public Task<PersistentTask<TParameters, TState>> ScheduleAsync<TParameters, TState>(
        PersistentTask<TParameters, TState> task,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var scheduler = new PersistentTaskScheduler();
        var task = CopyDirectoryTask.Create(new CopyDirectoryTask.Params
        {
            SourceDirectoryPath = "C:/etc/tmp/pt/srcdir",
            DestinationDirectoryPath = "C:/etc/tmp/pt/dstdir"
        });
        task = await scheduler.ScheduleAsync(task, CancellationToken.None);
    }
}
