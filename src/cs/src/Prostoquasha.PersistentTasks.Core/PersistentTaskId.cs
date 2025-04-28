using System;

namespace Prostoquasha.PersistentTasks.Core;

public readonly record struct PersistentTaskId
{
    private readonly string _value;

    public PersistentTaskId(string value)
    {
        _value = value;
    }

    public override string ToString()
    {
        return _value;
    }

    public static PersistentTaskId Create()
    {
        return new PersistentTaskId(Guid.NewGuid().ToString("N"));
    }
}
