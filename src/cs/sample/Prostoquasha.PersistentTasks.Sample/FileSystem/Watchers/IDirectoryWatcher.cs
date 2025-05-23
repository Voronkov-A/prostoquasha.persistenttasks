using System;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem.Watchers;

internal interface IDirectoryWatcher
{
    event EventHandler<FileSystemEntryCreatedEventArgs> EntryCreated;

    event EventHandler<FileSystemEntryDeletedEventArgs> EntryDeleted;

    event EventHandler<FileSystemEntryChangedEventArgs> EntryChanged;

    event EventHandler<FileSystemEntryRenamedEventArgs> EntryRenamed;
}
