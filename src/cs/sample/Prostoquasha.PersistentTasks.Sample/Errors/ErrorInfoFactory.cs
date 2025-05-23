using Prostoquasha.PersistentTasks.Core;

namespace Prostoquasha.PersistentTasks.Sample.Errors;

internal static class ErrorInfoFactory
{
    public static ErrorInfo Unknown()
    {
        return new ErrorInfo(code: "unknown", detail: "Unknown error has occurred.", data: "");
    }
}
