using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Core;

public class PersistentTaskConditionDisjunction
{
    public required WaitingPersistentTaskCondition FirstOperand { get; init; }

    public required WaitingForChildrenPersistentTaskCondition SecondOperand { get; init; }
}

public class WaitingPersistentTaskCondition
{
    public required PersistentTaskStatus StatusEquals { get; init; }

    public required DateTimeOffset ContinueAfterIsNotGreaterThan { get; init; }
}

public class WaitingForChildrenPersistentTaskCondition
{
    public required PersistentTaskStatus StatusEquals { get; init; }

    public required DateTimeOffset ContinueAfterIsNotGreaterThan { get; init; }

    public required IReadOnlyCollection<PersistentTaskStatus> ChildrenStatusesAreIn { get; init; }
}

public interface IPersistentTaskRepository
{
    //Task<> LockOrDefaultAsync(CancellationToken cancellationToken);

    Task<IPersistentTask?> SetStatusAndGetOrDefaultAsync(
        PersistentTaskConditionDisjunction filter,
        PersistentTaskStatus newStatus,
        CancellationToken cancellationToken);

    Task UpdateAsync(PersistentTaskId id, SetPersistentTaskStatusRequest request, CancellationToken cancellationToken);

    Task UpdateAsync(PersistentTaskId id, SetPersistentTaskStateRequest request, CancellationToken cancellationToken);

    Task UpdateAsync(PersistentTaskId id, SetPersistentTaskResultRequest request, CancellationToken cancellationToken);

    Task UpdateAsync(
        PersistentTaskId id,
        SetPersistentTaskFailedAttemptRequest request,
        CancellationToken cancellationToken);
}

public class SetPersistentTaskStatusRequest
{
    public required PersistentTaskStatus Status { get; init; }
}

public class SetPersistentTaskStateRequest
{
    public required PersistentTaskStatus Status { get; init; }

    public required SerializedData State { get; init; }

    public required DateTimeOffset? ContinueAfter { get; init; }
}

public class SetPersistentTaskResultRequest
{
    public required PersistentTaskStatus Status { get; init; }

    public required SerializedData Result { get; init; }
}

public class SetPersistentTaskFailedAttemptRequest
{
    public required PersistentTaskStatus Status { get; init; }

    public required FailedAttempt FailedAttempt { get; init; }

    public required DateTimeOffset? ContinueAfter { get; init; }
}

public readonly struct SerializedData(string typeKey, byte[] bytes)
{
    public string TypeKey { get; } = typeKey;

    public byte[]? Bytes { get; } = bytes;
}

public interface ISerializer
{
    SerializedData Serialize<T>(T? value);

    object? Deserialize(byte[]? bytes);
}

internal sealed class PersistentTaskProcessor
{
    private Task _execution;
    private readonly IPersistentTaskRepository _persistentTaskRepository;
    private readonly TimeProvider _timeProvider;
    private readonly PersistentTaskProcessorSettings _settings;
    private readonly Dictionary<Type, object> _executors;
    private readonly ISerializer _serializer;

    public void Start()
    {
        _execution = RunAsync(CancellationToken.None);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var task = await AcquireTaskAsync(cancellationToken);

            if (task.IsCancellationRequested)
            {
                await _persistentTaskRepository.UpdateAsync(
                    task.Id,
                    new SetPersistentTaskStatusRequest
                    {
                        Status = PersistentTaskStatus.Canceled
                    },
                    cancellationToken);
                continue;
            }


        }
    }


    private async Task ExecuteAsync<TParameters, TState, TResult>(
        PersistentTask<TParameters, TState, TResult> task,
        CancellationToken cancellationToken)
    {
        var executor = (IPersistentTaskExecutor<TParameters, TState, TResult>)_executors[task.GetType()];
        var result = await executor.ExecuteAsync(task, cancellationToken);

        switch (result.Command)
        {
            case ExecutionCommand.Continue:
                {
                    await _persistentTaskRepository.UpdateAsync(
                        task.Id,
                        new SetPersistentTaskStateRequest
                        {
                            Status = PersistentTaskStatus.Waiting,
                            State = _serializer.Serialize(result.NextState),
                            ContinueAfter = result.ContinueAfter
                        },
                        cancellationToken);
                    break;
                }
            case ExecutionCommand.Succeed:
                {
                    await _persistentTaskRepository.UpdateAsync(
                        task.Id,
                        new SetPersistentTaskResultRequest
                        {
                            Status = PersistentTaskStatus.Succeeded,
                            Result = _serializer.Serialize(result.Result)
                        },
                        cancellationToken);
                    break;
                }
            case ExecutionCommand.Fail:
                {
                    var error = result.Error
                        ?? throw new InvalidOperationException($"Command is {result.Command}, but error is null.");
                    var failedAttempt
                        = task.FailedAttempt != null
                        && task.FailedAttempt.Error.Code == error.Code
                        && task.FailedAttempt.Error.EqualityContract == error.EqualityContract
                        ? new FailedAttempt(error, task.FailedAttempt.ErrorRepeatCount + 1)
                        : new FailedAttempt(error, 1);

                    await _persistentTaskRepository.UpdateAsync(
                        task.Id,
                        new SetPersistentTaskFailedAttemptRequest
                        {
                            Status = PersistentTaskStatus.Failed,
                            FailedAttempt = failedAttempt,
                            ContinueAfter = result.ContinueAfter
                        },
                        cancellationToken);
                    break;
                }
            case ExecutionCommand.WaitForAllChildren:
                {
                    await _persistentTaskRepository.UpdateAsync(
                        task.Id,
                        new SetPersistentTaskStateRequest
                        {
                            Status = PersistentTaskStatus.WaitingForAllChildren,
                            State = _serializer.Serialize(result.NextState),
                            ContinueAfter = result.ContinueAfter
                        },
                        cancellationToken);
                    break;
                }
            case ExecutionCommand.WaitForAnyChild:
                {
                    await _persistentTaskRepository.UpdateAsync(
                        task.Id,
                        new SetPersistentTaskStateRequest
                        {
                            Status = PersistentTaskStatus.WaitingForAnyChild,
                            State = _serializer.Serialize(result.NextState),
                            ContinueAfter = result.ContinueAfter
                        },
                        cancellationToken);
                    break;
                }
            case ExecutionCommand.Cancel:
                {
                    await _persistentTaskRepository.UpdateAsync(
                        task.Id,
                        new SetPersistentTaskStatusRequest
                        {
                            Status = PersistentTaskStatus.Canceled
                        },
                        cancellationToken);
                    break;
                }
            default:
                {
                    throw new InvalidOperationException($"{nameof(ExecutionCommand)}.{task.Status} is not supported.");
                }
        }
    }

    private async Task<IPersistentTask> AcquireTaskAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var now = _timeProvider.GetUtcNow();

            var task = await _persistentTaskRepository.SetStatusAndGetOrDefaultAsync(
                filter: new PersistentTaskConditionDisjunction
                {
                    FirstOperand = new WaitingPersistentTaskCondition
                    {
                        StatusEquals = PersistentTaskStatus.Waiting,
                        ContinueAfterIsNotGreaterThan = now
                    },
                    SecondOperand = new WaitingForChildrenPersistentTaskCondition
                    {
                        StatusEquals = PersistentTaskStatus.WaitingForChildren,
                        ContinueAfterIsNotGreaterThan = now,
                        ChildrenStatusesAreIn = new[]
                        {
                            PersistentTaskStatus.Succeeded,
                            PersistentTaskStatus.Failed,
                            PersistentTaskStatus.Canceled
                        }
                    }
                },
                newStatus: PersistentTaskStatus.Executing,
                cancellationToken: cancellationToken);

            if (task != null)
            {
                return task;
            }

            await _timeProvider.Delay(_settings.DefaultTaskPollingPeriod, cancellationToken);
        }
    }
}
