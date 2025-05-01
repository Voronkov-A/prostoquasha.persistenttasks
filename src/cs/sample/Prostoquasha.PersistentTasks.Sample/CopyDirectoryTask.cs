using Prostoquasha.PersistentTasks.Core;
using Prostoquasha.PersistentTasks.Core.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample;

internal static class CopyDirectoryTask
{
    public static PersistentTask<Params, IState> Create(Params parameters, PersistentTaskId? parentId = null)
    {
        return new PersistentTask<Params, IState>(parameters, new IState.CollectingEntries(), parentId);
    }

    public sealed class Params
    {
        public required string SourceDirectoryPath { get; init; }

        public required string DestinationDirectoryPath { get; init; }
    }

    public interface IState
    {
        [PersistentTaskState]
        public sealed class CollectingEntries : IState;

        [PersistentTaskState(WaitingMode = WaitingMode.WhenAll)]
        public sealed class WaitingEntriesToCopy : IState, IWaitingPersistentTaskState
        {
            public required IReadOnlyCollection<PersistentTaskId> ExpectedTaskIds { get; init; }
        }

        [PersistentTaskState(IsFinal = true)]
        public sealed class Completed : IState;
    }

    public sealed class Executor(IPersistentTaskScheduler persistentTaskScheduler)
    {
        private readonly IPersistentTaskScheduler _persistentTaskScheduler = persistentTaskScheduler;

        public async Task<ExecutionResult<IState>> ExecuteAsync(
            PersistentTask<Params, IState> task,
            CancellationToken cancellationToken)
        {
            switch (task.State)
            {
                case IState.CollectingEntries:
                    var files = Directory.GetFiles(
                        task.Parameters.SourceDirectoryPath,
                        "*",
                        SearchOption.TopDirectoryOnly);
                    var directories = Directory.GetDirectories(
                        task.Parameters.SourceDirectoryPath,
                        "*",
                        SearchOption.TopDirectoryOnly);

                    var copyEntryTaskIds = new List<PersistentTaskId>();

                    foreach (var file in files)
                    {
                        var destinationFilePath = Path.Combine(
                            task.Parameters.DestinationDirectoryPath,
                            Path.GetFileName(file));
                        var copyFileTask = CopyFileTask.Create(
                            new CopyFileTask.Params
                            {
                                SourceFilePath = file,
                                DestinationFilePath = destinationFilePath
                            },
                            parentId: task.Id);
                        copyFileTask = await _persistentTaskScheduler.ScheduleAsync(copyFileTask, cancellationToken);
                        copyEntryTaskIds.Add(copyFileTask.Id);
                    }

                    foreach (var directory in directories)
                    {
                        var destinationDirectoryPath = Path.Combine(
                            task.Parameters.DestinationDirectoryPath,
                            Path.GetFileName(directory));
                        var copyDirectoryTask = Create(
                            new Params
                            {
                                SourceDirectoryPath = directory,
                                DestinationDirectoryPath = destinationDirectoryPath
                            },
                            parentId: task.Id);
                        copyDirectoryTask = await _persistentTaskScheduler.ScheduleAsync(
                            copyDirectoryTask,
                            cancellationToken);
                        copyEntryTaskIds.Add(copyDirectoryTask.Id);
                    }

                    return ExecutionResult.Continue<IState>(new IState.WaitingEntriesToCopy
                    {
                        ExpectedTaskIds = copyEntryTaskIds
                    });
                case IState.WaitingEntriesToCopy:
                    return ExecutionResult.Succeed<IState>(new IState.Completed());
                default:
                    throw new InvalidOperationException($"State {task.State.GetType()} is not supported.");
            }
        }
    }
}
