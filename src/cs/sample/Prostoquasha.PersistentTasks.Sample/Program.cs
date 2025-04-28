using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        await CopyDirectoryAsync("C:/etc/tmp/pt/srcdir", "C:/etc/tmp/pt/dstdir", CancellationToken.None);

        /*var t = new PersistentTask<int>();
        await t;
        Console.WriteLine("Hello, World!");*/

    }

    //public class

    public static async Task CopyFileAsync(
        string sourceFilePath,
        string destinationFilePath,
        CancellationToken cancellationToken)
    {
        var content = await File.ReadAllBytesAsync(sourceFilePath, cancellationToken);
        await File.WriteAllBytesAsync(destinationFilePath, content, cancellationToken);
    }

    public static async Task CopyDirectoryAsync(
        string sourceDirectoryPath,
        string destinationDirectoryPath,
        CancellationToken cancellationToken)
    {
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var destinationFilePath = Path.Combine(destinationDirectoryPath, Path.GetFileName(file));
            await CopyFileAsync(file, destinationFilePath, cancellationToken);
        }
    }



    public interface IFoo
    {
        public record Bar : IFoo;
    }


    public class CopyFileTaskParams
    {
        public required string SourceFilePath { get; init; }

        public required string DestinationFilePath { get; init; }
    }

    public abstract record CopyFileTaskState
    {
        public record Copying : CopyFileTaskState;
    }

    public readonly record struct PersistentTaskId
    {
        private readonly string _value;

        public PersistentTaskId(string value)
        {
            _value = value;
        }

        public static PersistentTaskId Create()
        {
            return new PersistentTaskId(Guid.NewGuid().ToString("N"));
        }
    }

    public interface IPersistentTaskState
    {
    }

    public interface IWaitingForAllPersistentTaskState : IPersistentTaskState
    {
        IReadOnlyCollection<PersistentTaskId> DependencyIds { get; }
    }

    public interface IErrorPersistentTaskState : IPersistentTaskState
    {

    }

    public class CopyFileTask
    {
        public CopyFileTask(CopyFileTaskParams parameters)
        {
            Id = PersistentTaskId.Create();
            Parameters = parameters;
            State = new CopyFileTaskState.Copying();
        }

        public PersistentTaskId Id { get; }

        public CopyFileTaskParams Parameters { get; }

        public CopyFileTaskState? State { get; }

        public IReadOnlyCollection<PersistentTaskId>
    }

    public class CopyFileTaskExecutor
    {
        public async Task<CopyDirectoryTaskState?> ExecuteAsync(
            CopyDirectoryTaskParams parameters,
            CopyDirectoryTaskState state,
            CancellationToken cancellationToken)
        {
            switch (state)
            {
                case CopyDirectoryTaskState.CreatingFileTasks:
                    {
                        var content = await File.ReadAllBytesAsync(parameters.SourceDirectoryPath, cancellationToken);
                        await File.WriteAllBytesAsync(parameters.DestinationDirectoryPath, content, cancellationToken);
                        return null;
                    }
                default:
                    {
                        throw new InvalidOperationException($"{state.GetType} is not supported.");
                    }
            }
        }
    }

    // waiting,

    public class CopyDirectoryTaskParams
    {
        public required string SourceDirectoryPath { get; init; }

        public required string DestinationDirectoryPath { get; init; }
    }

    public abstract record CopyDirectoryTaskState
    {
        public record CreatingFileTasks : CopyDirectoryTaskState;

        public record Waiting(IReadOnlyCollection<string> FilePaths, int FileIndex) : CopyDirectoryTaskState;
    }

    public class CopyDirectoryTaskExecutor
    {
        public async Task<CopyDirectoryTaskState?> ExecuteAsync(
            CopyDirectoryTaskParams parameters,
            CopyDirectoryTaskState state,
            CancellationToken cancellationToken)
        {
            switch (state)
            {
                case CopyDirectoryTaskState.CreatingFileTasks:
                    {
                        var files = Directory.GetFiles(
                            parameters.SourceDirectoryPath,
                            "*",
                            SearchOption.TopDirectoryOnly);

                        var copyFileTasks = files.Select(x => new CopyFileTask(new CopyFileTaskParams
                        {
                            SourceFilePath = x,
                            DestinationFilePath = Path.Combine(
                                parameters.DestinationDirectoryPath,
                                Path.GetFileName(x))
                        }));

                        return new CopyDirectoryTaskState.Waiting(files, 0);
                    }
                case CopyDirectoryTaskState.Waiting waiting:
                    {
                        return
                    }
            }
        }
    }

    public class PersistentTaskScheduler
    {

    }

    /*public sealed class PersistentTaskAwaiter<T> : INotifyCompletion
    {
        public bool IsCompleted => false;

        public T GetResult() => throw new NotImplementedException();

        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class PersistentTask<T>
    {
        public PersistentTaskAwaiter<T> GetAwaiter()
        {
            return new PersistentTaskAwaiter<T>();
        }
    }*/
}
