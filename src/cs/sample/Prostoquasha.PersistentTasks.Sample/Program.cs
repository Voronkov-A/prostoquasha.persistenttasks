using Prostoquasha.PersistentTasks.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample;

internal sealed class PersistentTaskScheduler : IPersistentTaskScheduler
{
    public Task<PersistentTask<TParameters, TState, TResult>> ScheduleAsync<TParameters, TState, TResult>(
        PersistentTask<TParameters, TState, TResult> task,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task WaitForCompletionAsync<TParameters, TState, TResult>(
        PersistentTask<TParameters, TState, TResult> task,
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
        await scheduler.WaitForCompletionAsync(task, CancellationToken.None);
    }
}
