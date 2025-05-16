using Prostoquasha.PersistentTasks.Core;
using Prostoquasha.PersistentTasks.Core.Attributes;
using Prostoquasha.PersistentTasks.Sample.FileSystem;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample.Tasks;

internal static class SynchronizeFileTask
{
    public static PersistentTask<Params, IState, Unit> Create(Params parameters, PersistentTaskId? parentId = null)
    {
        return new PersistentTask<Params, IState, Unit>(
            PersistentTaskId.Create(),
            parameters,
            new IState.Synchronizing(),
            parentId);
    }

    public sealed class Params
    {
        public required Uri SourceFileUrl { get; init; }

        public required Uri DestinationFileUrl { get; init; }
    }

    public interface IState
    {
        [PersistentTaskState]
        public sealed class Synchronizing : IState;
    }

    public sealed class Executor(
        ISourceFileSystem sourceFileSystem,
        IDestinationFileSystem destinationFileSystem)
    {
        private readonly ISourceFileSystem _sourceFileSystem = sourceFileSystem;
        private readonly IDestinationFileSystem _destinationFileSystem = destinationFileSystem;

        public async Task<ExecutionResult<IState, Unit>> ExecuteAsync(
            PersistentTask<Params, IState, Unit> task,
            CancellationToken cancellationToken)
        {
            switch (task.State)
            {
                case IState.Synchronizing:
                    {
                        var sourceFile = await _sourceFileSystem.GetFileOrDefaultAsync(
                            task.Parameters.SourceFileUrl,
                            cancellationToken);
                        var destinationFile = await _destinationFileSystem.GetFileOrDefaultAsync(
                            task.Parameters.DestinationFileUrl,
                            cancellationToken);

                        if (sourceFile == null)
                        {
                            if (destinationFile != null)
                            {
                                await _destinationFileSystem.DeleteFileAsync(destinationFile.Url, cancellationToken);
                            }

                            return ExecutionResult.Succeed<IState>();
                        }

                        if (destinationFile != null
                            && destinationFile.Size == sourceFile.Size
                            && destinationFile.Hash == sourceFile.Hash)
                        {
                            return ExecutionResult.Succeed<IState>();
                        }

                        var content = await _sourceFileSystem.ReadFileAsync(sourceFile.Url, cancellationToken);
                        await _destinationFileSystem.WriteFileAsync(
                            task.Parameters.DestinationFileUrl,
                            content,
                            cancellationToken);
                        return ExecutionResult.Succeed<IState>();
                    }
                default:
                    {
                        throw new InvalidOperationException($"State {task.State.GetType()} is not supported.");
                    }
            }
        }
    }
}
