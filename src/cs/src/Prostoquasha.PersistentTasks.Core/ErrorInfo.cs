namespace Prostoquasha.PersistentTasks.Core;

public sealed class ErrorInfo(string code, string detail, string data)
{
    public string Code { get; } = code;

    public string Detail { get; } = detail;

    public string Data { get; } = data;
}
