using Prostoquasha.PersistentTasks.Core;
using Prostoquasha.PersistentTasks.Core.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample.Tasks;

internal static class SynchronizeRootDirectoryTask
{
    public static PersistentTask<Params, IState, Unit> Create(Params parameters, PersistentTaskId? parentId = null)
    {
        return new PersistentTask<Params, IState, Unit>(
            PersistentTaskId.Create(),
            parameters,
            new IState.SpawningSubtasks(),
            parentId);
    }

    public sealed class Params
    {
        public required Uri SourceDirectoryUrl { get; init; }

        public required Uri DestinationDirectoryUrl { get; init; }
    }

    public interface IState
    {
        [PersistentTaskState]
        public sealed class SpawningSubtasks : IState;

        [PersistentTaskState]
        public sealed class WaitingForSubtasksToComplete : IState;
    }

    public sealed class Executor(IPersistentTaskScheduler persistentTaskScheduler)
    {
        private readonly IPersistentTaskScheduler _persistentTaskScheduler = persistentTaskScheduler;

        public async Task<ExecutionResult<IState, Unit>> ExecuteAsync(
            PersistentTask<Params, IState, Unit> task,
            CancellationToken cancellationToken)
        {
            switch (task.State)
            {
                case IState.SpawningSubtasks:
                    var synchronizeDirectoryTask = SynchronizeDirectoryTask.Create(
                        new SynchronizeDirectoryTask.Params
                        {
                            SourceDirectoryUrl = task.Parameters.SourceDirectoryUrl,
                            DestinationDirectoryUrl = task.Parameters.DestinationDirectoryUrl
                        },
                        parentId: task.Id);
                    await _persistentTaskScheduler.ScheduleAsync(synchronizeDirectoryTask, cancellationToken);
                    return ExecutionResult.WaitForAllChildren<IState, Unit>(new IState.WaitingForSubtasksToComplete());
                case IState.WaitingForSubtasksToComplete:
                    return ExecutionResult.Succeed<IState>();
                default:
                    throw new InvalidOperationException($"State {task.State.GetType()} is not supported.");
            }
        }
    }
}
