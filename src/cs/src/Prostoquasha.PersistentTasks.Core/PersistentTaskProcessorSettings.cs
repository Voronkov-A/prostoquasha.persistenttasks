using System;

namespace Prostoquasha.PersistentTasks.Core;

public sealed class PersistentTaskProcessorSettings
{
    public required TimeSpan DefaultTaskPollingPeriod { get; init; }
}
