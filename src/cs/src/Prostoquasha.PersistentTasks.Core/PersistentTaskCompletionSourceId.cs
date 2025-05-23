using System;

namespace Prostoquasha.PersistentTasks.Core;

public readonly record struct PersistentTaskCompletionSourceId
{
    private readonly string _value;

    public PersistentTaskCompletionSourceId(string value)
    {
        _value = value;
    }

    public override string ToString()
    {
        return _value;
    }

    public static PersistentTaskCompletionSourceId Create()
    {
        return new PersistentTaskCompletionSourceId(Guid.NewGuid().ToString("N"));
    }
}
