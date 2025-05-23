namespace Prostoquasha.PersistentTasks.Core;

public sealed class ErrorInfo(
    string code,
    string detail,
    object? parameters,
    string? diagnosticData,
    string? equalityContract)
{
    public string Code { get; } = code;

    public string Detail { get; } = detail;

    public object? Parameters { get; } = parameters;

    public string? DiagnosticData { get; } = diagnosticData;

    public string? EqualityContract { get; } = equalityContract;
}
