using System;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem.Watchers;

public sealed class FileSystemEntryRenamedEventArgs(Uri oldUrl, Uri newUrl) : EventArgs
{
    public Uri OldUrl { get; } = oldUrl;

    public Uri NewUrl { get; } = newUrl;
}
