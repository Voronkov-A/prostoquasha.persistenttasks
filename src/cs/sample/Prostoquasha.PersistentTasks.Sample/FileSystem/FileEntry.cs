using System;
using System.Linq;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem;

internal sealed class FileEntry
{
    public FileEntry(Uri url, Md5Hash md5Hash, MemoryUnit size)
    {
        Url = url;
        Md5Hash = md5Hash;
        Size = size;
    }

    public Uri Url { get; }

    public Md5Hash Md5Hash { get; }

    public MemoryUnit Size { get; }

    public string Name => Url.Segments.Last();
}
