using System;
using System.Linq;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem;

internal sealed class FileEntry
{
    public FileEntry(Uri url, string hash, MemoryUnit size)
    {
        Url = url;
        Hash = hash;
        Size = size;
    }

    public Uri Url { get; }

    public string Hash { get; }

    public MemoryUnit Size { get; }

    public string Name => Url.Segments.Last();
}
