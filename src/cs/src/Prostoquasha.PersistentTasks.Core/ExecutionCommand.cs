namespace Prostoquasha.PersistentTasks.Core;

public enum ExecutionCommand
{
    Continue,
    Succeed,
    Fail,
    WaitForAllChildren,
    WaitForAnyChild,
    Cancel
}
