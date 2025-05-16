namespace Prostoquasha.PersistentTasks.Sample.FileSystem;

public readonly record struct MemoryUnit
{
    private readonly long _bytes;

    private MemoryUnit(long bytes)
    {
        _bytes = bytes;
    }

    public static MemoryUnit FromBytes(long bytes)
    {
        return new(bytes: bytes);
    }
}
