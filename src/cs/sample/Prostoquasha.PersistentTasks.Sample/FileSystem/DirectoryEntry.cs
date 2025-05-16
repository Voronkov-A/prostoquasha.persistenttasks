using System;
using System.Linq;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem;

internal sealed class DirectoryEntry
{
    public DirectoryEntry(Uri url)
    {
        Url = url;
    }

    public Uri Url { get; }

    public string Name => Url.Segments.Last();
}
