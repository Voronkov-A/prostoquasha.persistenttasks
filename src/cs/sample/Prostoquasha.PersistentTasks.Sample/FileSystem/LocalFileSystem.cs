using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem;

internal sealed class LocalFileSystem : ISourceFileSystem, IDestinationFileSystem
{
    public ValueTask CreateDirectoryAsync(Uri directoryUrl, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(ToPath(directoryUrl));
        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteDirectoryAsync(Uri directoryUrl, CancellationToken cancellationToken)
    {
        Directory.Delete(ToPath(directoryUrl), recursive: true);
        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteFileAsync(Uri fileUrl, CancellationToken cancellationToken)
    {
        File.Delete(ToPath(fileUrl));
        return ValueTask.CompletedTask;
    }

    public IAsyncEnumerable<DirectoryEntry> EnumerateDirectoriesAsync(
        Uri rootDirectoryPath,
        EntryOrder order,
        CancellationToken cancellationToken)
    {
        var directories = new DirectoryInfo(ToPath(rootDirectoryPath)).EnumerateDirectories();

        directories = order switch
        {
            EntryOrder.ByUrl => directories.OrderBy(x => x.Name),
            _ => throw new InvalidOperationException($"{nameof(EntryOrder)}.{order} is not supported.")
        };

        return directories.Select(ToDirectoryEntry).ToAsyncEnumerable();
    }

    public IAsyncEnumerable<FileEntry> EnumerateFilesAsync(
        Uri rootDirectoryPath,
        EntryOrder order,
        CancellationToken cancellationToken)
    {
        var files = new DirectoryInfo(ToPath(rootDirectoryPath)).EnumerateFiles();

        files = order switch
        {
            EntryOrder.ByUrl => files.OrderBy(x => x.Name),
            _ => throw new InvalidOperationException($"{nameof(EntryOrder)}.{order} is not supported.")
        };

        return files.Select(ToFileEntry).ToAsyncEnumerable();
    }

    public ValueTask<DirectoryEntry?> GetDirectoryOrDefaultAsync(Uri fileUrl, CancellationToken cancellationToken)
    {
        var directory = new DirectoryInfo(ToPath(fileUrl));
        var result = directory.Exists ? ToDirectoryEntry(directory) : null;
        return ValueTask.FromResult(result);
    }

    public ValueTask<FileEntry?> GetFileOrDefaultAsync(Uri fileUrl, CancellationToken cancellationToken)
    {
        var file = new FileInfo(ToPath(fileUrl));
        var result = file.Exists ? ToFileEntry(file) : null;
        return ValueTask.FromResult(result);
    }

    public ValueTask<Stream> ReadFileAsync(Uri fileUrl, CancellationToken cancellationToken)
    {
        var result = File.Open(ToPath(fileUrl), FileMode.Open, FileAccess.Read, FileShare.Read);
        return ValueTask.FromResult<Stream>(result);
    }

    public async ValueTask WriteFileAsync(Uri fileUrl, Stream content, CancellationToken cancellationToken)
    {
        await using var output = File.Open(ToPath(fileUrl), FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(output, cancellationToken);
    }

    private static DirectoryEntry ToDirectoryEntry(DirectoryInfo directoryInfo)
    {
        return new DirectoryEntry(ToUrl(directoryInfo.FullName));
    }

    private static FileEntry ToFileEntry(FileInfo fileInfo)
    {
        return new FileEntry(
            url: ToUrl(fileInfo.FullName),
            md5Hash: CalculateMd5Hash(fileInfo),
            size: MemoryUnit.FromBytes(fileInfo.Length));
    }

    private static Md5Hash CalculateMd5Hash(FileInfo fileInfo)
    {
        using var file = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new Md5Hash(file);
    }

    private static string ToPath(Uri url)
    {
        return url.LocalPath;
    }

    private static Uri ToUrl(string path)
    {
        return new Uri(path);
    }
}
