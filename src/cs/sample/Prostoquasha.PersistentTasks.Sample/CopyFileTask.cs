using Prostoquasha.PersistentTasks.Core;
using Prostoquasha.PersistentTasks.Core.Attributes;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample;

internal static class CopyFileTask
{
    public static PersistentTask<Params, IState> Create(Params parameters, PersistentTaskId? parentId = null)
    {
        return new PersistentTask<Params, IState>(parameters, new IState.CopyingFile(), parentId);
    }

    public sealed class Params
    {
        public required string SourceFilePath { get; init; }

        public required string DestinationFilePath { get; init; }
    }

    public interface IState
    {
        [PersistentTaskState(Name = "Copying file...")]
        public sealed class CopyingFile : IState;

        [PersistentTaskState(IsFinal = true)]
        public sealed class Completed : IState;
    }

    public sealed class Executor
    {
        public async Task<IState> ExecuteAsync(PersistentTask<Params, IState> task, CancellationToken cancellationToken)
        {
            switch (task.State)
            {
                case IState.CopyingFile:
                    var content = await File.ReadAllBytesAsync(task.Parameters.SourceFilePath, cancellationToken);
                    await File.WriteAllBytesAsync(task.Parameters.DestinationFilePath, content, cancellationToken);
                    return new IState.Completed();
                default:
                    throw new InvalidOperationException($"State {task.State.GetType()} is not supported.");
            }
        }
    }
}
