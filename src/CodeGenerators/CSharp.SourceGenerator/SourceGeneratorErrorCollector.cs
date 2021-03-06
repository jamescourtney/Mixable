using Microsoft.CodeAnalysis;
using Mixable.Schema;

namespace Mixable.SourceGenerator;

[ExcludeFromCodeCoverage]
internal record SourceGeneratorErrorCollector : IErrorCollector
{
    private readonly GeneratorExecutionContext context;
    private readonly string file;

    public SourceGeneratorErrorCollector(ref GeneratorExecutionContext context, string filePath)
    {
        this.context = context;
        this.file = filePath;
    }

    public bool HasErrors { get; private set; }

    public void Error(string message, string? path = null)
    {
        this.HasErrors = true;

        this.context.ReportDiagnostic(Diagnostic.Create(
            "MX0001",
            "Mixable",
            $"Message = '{message}' Path = '{path}'",
            severity: DiagnosticSeverity.Error,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            warningLevel: 0,
            isSuppressed: false,
            location: Location.Create(this.file, default, default)));
    }

    public void Info(string message, string? path = null)
    {
        this.context.ReportDiagnostic(Diagnostic.Create(
            "MX0003",
            "Mixable",
            $"Message = '{message}' Path = '{path}'",
            severity: DiagnosticSeverity.Info,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            warningLevel: 2,
            isSuppressed: false,
            location: Location.Create(this.file, default, default)));
    }

    public void Warning(string message, string? path = null)
    {
        this.context.ReportDiagnostic(Diagnostic.Create(
            "MX0002",
            "Mixable",
            $"Message = '{message}' Path = '{path}'",
            severity: DiagnosticSeverity.Warning,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            warningLevel: 1,
            isSuppressed: false,
            location: Location.Create(this.file, default, default)));
    }
}