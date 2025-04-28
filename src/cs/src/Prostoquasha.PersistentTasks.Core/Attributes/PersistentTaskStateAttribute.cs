using System;

namespace Prostoquasha.PersistentTasks.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class PersistentTaskStateAttribute : Attribute
{
}
