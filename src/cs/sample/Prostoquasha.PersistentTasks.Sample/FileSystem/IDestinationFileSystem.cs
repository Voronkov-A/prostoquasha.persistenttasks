using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem;

internal interface IDestinationFileSystem
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

    ValueTask<DirectoryEntry?> GetDirectoryOrDefaultAsync(Uri fileUrl, CancellationToken cancellationToken);

    ValueTask DeleteFileAsync(Uri fileUrl, CancellationToken cancellationToken);

    ValueTask DeleteDirectoryAsync(Uri directoryUrl, CancellationToken cancellationToken);

    ValueTask WriteFileAsync(Uri fileUrl, Stream content, CancellationToken cancellationToken);

    ValueTask CreateDirectoryAsync(Uri directoryUrl, CancellationToken cancellationToken);
}
