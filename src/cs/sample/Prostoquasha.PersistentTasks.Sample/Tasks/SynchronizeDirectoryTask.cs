using Prostoquasha.PersistentTasks.Core;
using Prostoquasha.PersistentTasks.Core.Attributes;
using Prostoquasha.PersistentTasks.Sample.FileSystem;
using Prostoquasha.PersistentTasks.Sample.Miscellaneous;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample.Tasks;

internal static class SynchronizeDirectoryTask
{
    public static PersistentTask<Params, IState, Unit> Create(Params parameters, PersistentTaskId? parentId = null)
    {
        return new PersistentTask<Params, IState, Unit>(
            PersistentTaskId.Create(),
            parameters,
            new IState.CollectingEntries(),
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
        public sealed class CollectingEntries : IState;

        [PersistentTaskState]
        public sealed class WaitingEntriesToSynchronize : IState;
    }

    public sealed class Executor(
        IPersistentTaskScheduler persistentTaskScheduler,
        ISourceFileSystem sourceFileSystem,
        IDestinationFileSystem destinationFileSystem)
    {
        private readonly IPersistentTaskScheduler _persistentTaskScheduler = persistentTaskScheduler;
        private readonly ISourceFileSystem _sourceFileSystem = sourceFileSystem;
        private readonly IDestinationFileSystem _destinationFileSystem = destinationFileSystem;

        public async Task<ExecutionResult<IState, Unit>> ExecuteAsync(
            PersistentTask<Params, IState, Unit> task,
            CancellationToken cancellationToken)
        {
            switch (task.State)
            {
                case IState.CollectingEntries:
                    {
                        var sourceRootDirectory = await _sourceFileSystem.GetDirectoryOrDefaultAsync(
                            task.Parameters.SourceDirectoryUrl,
                            cancellationToken);
                        var destinationRootDirectory = await _destinationFileSystem.GetDirectoryOrDefaultAsync(
                            task.Parameters.DestinationDirectoryUrl,
                            cancellationToken);

                        if (sourceRootDirectory == null)
                        {
                            if (destinationRootDirectory != null)
                            {
                                await _destinationFileSystem.DeleteDirectoryAsync(
                                    destinationRootDirectory.Url,
                                    cancellationToken);
                            }

                            return ExecutionResult.Succeed<IState>();
                        }

                        if (destinationRootDirectory == null)
                        {
                            await _destinationFileSystem.CreateDirectoryAsync(
                                task.Parameters.DestinationDirectoryUrl,
                                cancellationToken);
                        }

                        var sourceFiles = _sourceFileSystem.EnumerateFilesAsync(
                            task.Parameters.SourceDirectoryUrl,
                            EntryOrder.ByUrl,
                            cancellationToken);
                        var destinationFiles = _destinationFileSystem.EnumerateFilesAsync(
                            task.Parameters.DestinationDirectoryUrl,
                            EntryOrder.ByUrl,
                            cancellationToken);

                        await foreach (var (sourceFile, destinationFile) in sourceFiles
                            .FullJoinOrdered(destinationFiles, x => x.Name, cancellationToken))
                        {
                            if (sourceFile != null
                                && destinationFile != null
                                && sourceFile.Size == destinationFile.Size
                                && sourceFile.Md5Hash == destinationFile.Md5Hash)
                            {
                                continue;
                            }

                            if (sourceFile != null)
                            {
                                await SynchronizeFileAsync(
                                    task,
                                    sourceFile.Url,
                                    new Uri(task.Parameters.DestinationDirectoryUrl, sourceFile.Name),
                                    cancellationToken);
                                continue;
                            }

                            if (destinationFile != null)
                            {
                                await SynchronizeFileAsync(
                                    task,
                                    new Uri(task.Parameters.SourceDirectoryUrl, destinationFile.Name),
                                    destinationFile.Url,
                                    cancellationToken);
                                continue;
                            }
                        }

                        var sourceDirectories = _sourceFileSystem.EnumerateDirectoriesAsync(
                            task.Parameters.SourceDirectoryUrl,
                            EntryOrder.ByUrl,
                            cancellationToken);
                        var destinationDirectories = _destinationFileSystem.EnumerateDirectoriesAsync(
                            task.Parameters.DestinationDirectoryUrl,
                            EntryOrder.ByUrl,
                            cancellationToken);

                        await foreach (var (sourceDirectory, destinationDirectory) in sourceDirectories
                            .FullJoinOrdered(destinationDirectories, x => x.Name, cancellationToken))
                        {
                            if (sourceDirectory != null)
                            {
                                await SynchronizeDirectoryAsync(
                                    task,
                                    sourceDirectory.Url,
                                    new Uri(task.Parameters.DestinationDirectoryUrl, sourceDirectory.Name),
                                    cancellationToken);
                                continue;
                            }

                            if (destinationDirectory != null)
                            {
                                await SynchronizeDirectoryAsync(
                                    task,
                                    new Uri(task.Parameters.SourceDirectoryUrl, destinationDirectory.Name),
                                    destinationDirectory.Url,
                                    cancellationToken);
                                continue;
                            }
                        }

                        return ExecutionResult.WaitForAllChildren<IState, Unit>(
                            new IState.WaitingEntriesToSynchronize());
                    }
                case IState.WaitingEntriesToSynchronize:
                    {
                        return ExecutionResult.Succeed<IState>();
                    }
                default:
                    {
                        throw new InvalidOperationException($"State {task.State.GetType()} is not supported.");
                    }
            }
        }

        private async Task SynchronizeFileAsync(
            PersistentTask<Params, IState, Unit> parentTask,
            Uri sourceFileUrl,
            Uri destinationFileUrl,
            CancellationToken cancellationToken)
        {
            var synchronizeFileTask = SynchronizeFileTask.Create(
                new SynchronizeFileTask.Params
                {
                    SourceFileUrl = sourceFileUrl,
                    DestinationFileUrl = destinationFileUrl
                },
                parentId: parentTask.Id);
            await _persistentTaskScheduler.ScheduleAsync(synchronizeFileTask, cancellationToken);
        }

        private async Task SynchronizeDirectoryAsync(
            PersistentTask<Params, IState, Unit> parentTask,
            Uri sourceDirectoryUrl,
            Uri destinationDirectoryUrl,
            CancellationToken cancellationToken)
        {
            var synchronizeDirectoryTask = Create(
                new Params
                {
                    SourceDirectoryUrl = sourceDirectoryUrl,
                    DestinationDirectoryUrl = destinationDirectoryUrl
                },
                parentId: parentTask.Id);
            await _persistentTaskScheduler.ScheduleAsync(synchronizeDirectoryTask, cancellationToken);
        }
    }
}
