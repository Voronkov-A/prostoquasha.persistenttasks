using System;

namespace Prostoquasha.PersistentTasks.Core;

public sealed class ExecutionResult<TState>
{
    private ExecutionResult(
        TState? nextState,
        ErrorInfo? error,
        DateTimeOffset? continueAfter,
        ExecutionCommand command)
    {
        NextState = nextState;
        Error = error;
        ContinueAfter = continueAfter;
        Command = command;
    }

    public TState? NextState { get; }

    public ErrorInfo? Error { get; }

    public DateTimeOffset? ContinueAfter { get; }

    public ExecutionCommand Command { get; }

    internal static ExecutionResult<TState> Continue(TState nextState, DateTimeOffset? continueAfter)
    {
        return new ExecutionResult<TState>(nextState, null, continueAfter, ExecutionCommand.Continue);
    }

    internal static ExecutionResult<TState> Retry(ErrorInfo error, DateTimeOffset? continueAfter)
    {
        return new ExecutionResult<TState>(default, error, continueAfter, ExecutionCommand.Continue);
    }

    internal static ExecutionResult<TState> Fail(ErrorInfo error)
    {
        return new ExecutionResult<TState>(default, error, null, ExecutionCommand.Fail);
    }

    internal static ExecutionResult<TState> Succeed(TState result)
    {
        return new ExecutionResult<TState>(result, null, null, ExecutionCommand.Succeed);
    }
}

public static class ExecutionResult
{
    public static ExecutionResult<TState> Continue<TState>(TState nextState, DateTimeOffset? continueAfter = null)
    {
        return ExecutionResult<TState>.Continue(nextState, continueAfter);
    }

    public static ExecutionResult<TState> Retry<TState>(ErrorInfo error, DateTimeOffset? continueAfter = null)
    {
        return ExecutionResult<TState>.Retry(error, continueAfter);
    }

    public static ExecutionResult<TState> Fail<TState>(ErrorInfo error)
    {
        return ExecutionResult<TState>.Fail(error);
    }

    public static ExecutionResult<TState> Succeed<TState>(TState result)
    {
        return ExecutionResult<TState>.Succeed(result);
    }
}
