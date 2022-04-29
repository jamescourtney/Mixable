namespace UnitTests;

public class TestErrorCollector : IErrorCollector
{
    public HashSet<(string msg, string? path)> Errors = new();
    public HashSet<(string msg, string? path)> Warnings = new();
    public HashSet<(string msg, string? path)> Infos = new();

    public bool HasErrors => this.Errors.Count > 0;

    public void Error(string message, string? path = null)
    {
        this.Errors.Add((message, path));
    }

    public void Info(string message, string? path = null)
    {
        this.Infos.Add((message, path));
    }

    public void Warning(string message, string? path = null)
    {
        this.Warnings.Add((message, path));
    }

    public void Reset()
    {
        this.Errors.Clear();
        this.Warnings.Clear();
        this.Infos.Clear();
    }
}