using System;

namespace Prostoquasha.PersistentTasks.Sample.Commands;

internal sealed class StartSynchronizationCommand(Uri sourceDirectoryUrl, Uri destinationDirectoryUrl)
{
    public Uri SourceDirectoryUrl { get; } = sourceDirectoryUrl;

    public Uri DestinationDirectoryUrl { get; } = destinationDirectoryUrl;
}
