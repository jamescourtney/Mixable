namespace Mixable.Schema;

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

[ExcludeFromCodeCoverage]
public static class IErrorCollectorExtensions
{
    public static void Error(this IErrorCollector ec, string message, XElement element) 
        => ec.Error(message, element.GetDocumentPath());

    public static void Warning(this IErrorCollector ec, string message, XElement element) 
        => ec.Warning(message, element.GetDocumentPath());

    public static void Info(this IErrorCollector ec, string message, XElement element) 
        => ec.Info(message, element.GetDocumentPath());
}