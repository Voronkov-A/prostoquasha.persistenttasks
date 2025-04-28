namespace Prostoquasha.PersistentTasks.Core;

public sealed class PersistentTask<TParameters, TState>
{
    public PersistentTask(TParameters parameters, TState state, PersistentTaskId? parentId = null)
    {
        Id = PersistentTaskId.Create();
        Parameters = parameters;
        State = state;
        ParentId = parentId;
    }

    public PersistentTaskId Id { get; }

    public PersistentTaskId? ParentId { get; }

    public TParameters Parameters { get; }

    public TState State { get; }
}
