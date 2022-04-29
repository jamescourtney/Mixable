namespace Mixable.Schema;

/// <summary>
/// A correct (but useless) implementation of IErrorCollector.
/// </summary>
[ExcludeFromCodeCoverage]
internal class NoOpErrorCollector : IErrorCollector
{
    public bool HasErrors { get; private set; }

    public void Error(string message, string? path = null)
    {
        this.HasErrors = true;
    }

    public void Info(string message, string? path = null)
    {
    }

    public void Warning(string message, string? path = null)
    {
    }
}