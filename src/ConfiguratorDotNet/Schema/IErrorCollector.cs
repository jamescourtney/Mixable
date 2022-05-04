namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Collects errors while traversing a schema.
/// </summary>
public interface IErrorCollector
{
    /// <summary>
    /// Indicates if any errors have been collected.
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    /// Collects an error message.
    /// </summary>
    void Error(string message, string? path = null);

    /// <summary>
    /// Collects a warning message.
    /// </summary>
    void Warning(string message, string? path = null);

    /// <summary>
    /// Collects an informational message.
    /// </summary>
    void Info(string message, string? path = null);
}