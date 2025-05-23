using Prostoquasha.PersistentTasks.Core;
using Prostoquasha.PersistentTasks.Core.Attributes;
using Prostoquasha.PersistentTasks.Sample.Errors;
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
        public sealed class WaitingForSubtasksToComplete(
            PersistentTaskId synchronizeDirectoryTaskId,
            PersistentTaskCompletionSourceId stopTaskCompletionSourceId) :
            IState
        {
            public PersistentTaskId SynchronizeDirectoryTaskId { get; } = synchronizeDirectoryTaskId;

            public PersistentTaskCompletionSourceId StopTaskCompletionSourceId { get; } = stopTaskCompletionSourceId;
        }
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
                    {
                        var synchronizeDirectoryTask = SynchronizeDirectoryTask.Create(
                            new SynchronizeDirectoryTask.Params
                            {
                                SourceDirectoryUrl = task.Parameters.SourceDirectoryUrl,
                                DestinationDirectoryUrl = task.Parameters.DestinationDirectoryUrl
                            },
                            parentId: task.Id);
                        synchronizeDirectoryTask = await _persistentTaskScheduler.ScheduleAsync(
                            synchronizeDirectoryTask,
                            cancellationToken);

                        var stopTaskCompletionSource = new PersistentTaskCompletionSource<Unit>(
                            PersistentTaskCompletionSourceId.Create(),
                            parentId: task.Id);
                        stopTaskCompletionSource = await _persistentTaskScheduler.RegisterAsync(
                            stopTaskCompletionSource,
                            cancellationToken);

                        return ExecutionResult.WaitForAnyChild<IState, Unit>(new IState.WaitingForSubtasksToComplete(
                            synchronizeDirectoryTask.Id,
                            stopTaskCompletionSource.Id));
                    }
                case IState.WaitingForSubtasksToComplete waitingForSubtasksToComplete:
                    {
                        var synchronizeDirectoryTask = await _persistentTaskScheduler
                            .GetOrDefaultAsync<SynchronizeDirectoryTask.Params, SynchronizeDirectoryTask.IState, Unit>(
                                waitingForSubtasksToComplete.SynchronizeDirectoryTaskId,
                                cancellationToken);
                        var stopTaskCompletionSource = await _persistentTaskScheduler
                            .GetOrDefaultAsync<Unit>(
                                waitingForSubtasksToComplete.StopTaskCompletionSourceId,
                                cancellationToken);

                        return synchronizeDirectoryTask.Status switch
                        {
                            PersistentTaskStatus.Failed =>
                                ExecutionResult.Fail<IState, Unit>(
                                    synchronizeDirectoryTask.FailedAttempt?.Error ?? ErrorInfoFactory.Unknown()),
                            PersistentTaskStatus.Canceled =>
                                ExecutionResult.Cancel<IState>(),
                            PersistentTaskStatus.Succeeded when stopTaskCompletionSource.Status.IsCompleted() =>
                                ExecutionResult.Succeed<IState>(),
                            _ =>
                                ExecutionResult.WaitForAnyChild<IState, Unit>(
                                    new IState.WaitingForSubtasksToComplete(
                                        synchronizeDirectoryTask.Id,
                                        stopTaskCompletionSource.Id))
                        };
                    }
                default:
                    {
                        throw new InvalidOperationException($"State {task.State.GetType()} is not supported.");
                    }
            }
        }
    }
}
