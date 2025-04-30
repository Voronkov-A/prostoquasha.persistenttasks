using System;

namespace Prostoquasha.PersistentTasks.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class PersistentTaskStateAttribute : Attribute
{
    public string? Name { get; init; }

    public bool IsFinal { get; init; }

    public WaitingMode WaitingMode { get; init; } = WaitingMode.None;
}
