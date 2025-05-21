using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem;

internal interface ISourceFileSystem
{
    IAsyncEnumerable<DirectoryEntry> EnumerateDirectoriesAsync(
        Uri rootDirectoryPath,
        EntryOrder order,
        CancellationToken cancellationToken);

    IAsyncEnumerable<FileEntry> EnumerateFilesAsync(
        Uri rootDirectoryPath,
        EntryOrder order,
        CancellationToken cancellationToken);

    ValueTask<FileEntry?> GetFileOrDefaultAsync(Uri fileUrl, CancellationToken cancellationToken);

    ValueTask<DirectoryEntry?> GetDirectoryOrDefaultAsync(Uri directoryUrl, CancellationToken cancellationToken);

    ValueTask<Stream> ReadFileAsync(Uri fileUrl, CancellationToken cancellationToken);
}
