using System;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem.Watchers;

public sealed class FileSystemEntryChangedEventArgs(Uri url) : EventArgs
{
    public Uri Url { get; } = url;
}
