using System.Collections.Generic;

namespace Prostoquasha.PersistentTasks.Core;

public interface IWaitingPersistentTaskState
{
    IReadOnlyCollection<PersistentTaskId> ExpectedTaskIds { get; }
}
