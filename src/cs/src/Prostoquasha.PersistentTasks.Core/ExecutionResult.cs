using System;

namespace Prostoquasha.PersistentTasks.Core;

public sealed class ExecutionResult<TState, TResult>
{
    private ExecutionResult(
        TState? nextState,
        TResult? result,
        ErrorInfo? error,
        DateTimeOffset? continueAfter,
        ExecutionCommand command)
    {
        NextState = nextState;
        Result = result;
        Error = error;
        ContinueAfter = continueAfter;
        Command = command;
    }

    public TState? NextState { get; }

    public TResult? Result { get; }

    public ErrorInfo? Error { get; }

    public DateTimeOffset? ContinueAfter { get; }

    public ExecutionCommand Command { get; }

    internal static ExecutionResult<TState, TResult> Continue(TState nextState, DateTimeOffset? continueAfter)
    {
        return new ExecutionResult<TState, TResult>(nextState, default, null, continueAfter, ExecutionCommand.Continue);
    }

    internal static ExecutionResult<TState, TResult> Retry(ErrorInfo error, DateTimeOffset? continueAfter)
    {
        return new ExecutionResult<TState, TResult>(default, default, error, continueAfter, ExecutionCommand.Continue);
    }

    internal static ExecutionResult<TState, TResult> Fail(ErrorInfo error)
    {
        return new ExecutionResult<TState, TResult>(default, default, error, null, ExecutionCommand.Fail);
    }

    internal static ExecutionResult<TState, TResult> Succeed(TResult result)
    {
        return new ExecutionResult<TState, TResult>(default, result, null, null, ExecutionCommand.Succeed);
    }

    internal static ExecutionResult<TState, TResult> WaitForAllChildren(TState nextState, DateTimeOffset? continueAfter)
    {
        return new ExecutionResult<TState, TResult>(
            nextState,
            default,
            null,
            continueAfter,
            ExecutionCommand.WaitForAllChildren);
    }

    internal static ExecutionResult<TState, TResult> WaitForAnyChild(TState nextState, DateTimeOffset? continueAfter)
    {
        return new ExecutionResult<TState, TResult>(
            nextState,
            default,
            null,
            continueAfter,
            ExecutionCommand.WaitForAnyChild);
    }

    internal static ExecutionResult<TState, TResult> Cancel()
    {
        return new ExecutionResult<TState, TResult>(
            default,
            default,
            default,
            default,
            ExecutionCommand.Cancel);
    }
}

public static class ExecutionResult
{
    public static ExecutionResult<TState, TResult> Continue<TState, TResult>(
        TState nextState,
        DateTimeOffset? continueAfter = null)
    {
        return ExecutionResult<TState, TResult>.Continue(nextState, continueAfter);
    }

    public static ExecutionResult<TState, TResult> Retry<TState, TResult>(
        ErrorInfo error,
        DateTimeOffset? continueAfter = null)
    {
        return ExecutionResult<TState, TResult>.Retry(error, continueAfter);
    }

    public static ExecutionResult<TState, TResult> Fail<TState, TResult>(ErrorInfo error)
    {
        return ExecutionResult<TState, TResult>.Fail(error);
    }

    public static ExecutionResult<TState, TResult> Succeed<TState, TResult>(TResult result)
    {
        return ExecutionResult<TState, TResult>.Succeed(result);
    }

    public static ExecutionResult<TState, Unit> Succeed<TState>()
    {
        return ExecutionResult<TState, Unit>.Succeed(Unit.Value);
    }

    public static ExecutionResult<TState, TResult> WaitForAllChildren<TState, TResult>(
        TState nextState,
        DateTimeOffset? continueAfter = null)
    {
        return ExecutionResult<TState, TResult>.WaitForAllChildren(nextState, continueAfter);
    }

    public static ExecutionResult<TState, TResult> WaitForAnyChild<TState, TResult>(
        TState nextState,
        DateTimeOffset? continueAfter = null)
    {
        return ExecutionResult<TState, TResult>.WaitForAnyChild(nextState, continueAfter);
    }

    public static ExecutionResult<TState, TResult> Cancel<TState, TResult>()
    {
        return ExecutionResult<TState, TResult>.Cancel();
    }

    public static ExecutionResult<TState, Unit> Cancel<TState>()
    {
        return ExecutionResult<TState, Unit>.Cancel();
    }
}
