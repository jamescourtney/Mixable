namespace Mixable.Schema;

/// <summary>
/// Deduplicates status messages.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeduplicatingErrorCollector : IErrorCollector
{
    private readonly HashSet<(string, string?)> errors = new();
    private readonly HashSet<(string, string?)> warnings = new();
    private readonly HashSet<(string, string?)> infos = new();

    private readonly IErrorCollector? innerCollector;

    public DeduplicatingErrorCollector(IErrorCollector? innerCollector)
    {
        this.innerCollector = innerCollector;
    }

    public bool HasErrors => this.errors.Count > 0;

    public void Error(string message, string? path = null)
    {
        if (this.errors.Add((message, path)))
        {
            this.innerCollector?.Error(message, path);
        }
    }

    public void Info(string message, string? path = null)
    {
        if (this.infos.Add((message, path)))
        {
            this.innerCollector?.Info(message, path);
        }
    }

    public void Warning(string message, string? path = null)
    {
        if (this.warnings.Add((message, path)))
        {
            this.innerCollector?.Warning(message, path);
        }
    }
}