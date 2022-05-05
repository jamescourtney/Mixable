using ConfiguratorDotNet.Schema;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Diagnostics;

namespace SourceGenerator;

[Generator]
public class ConfiguratorDotNetCSharpSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Debugger.Launch();

        foreach (var additionalFile in context.AdditionalFiles)
        {
            try
            {
                if (additionalFile.Path.ToLowerInvariant().EndsWith(".cdn.xml"))
                {
                    ErrorCollector errorCollector = new ErrorCollector(
                        ref context,
                        additionalFile.Path);

                    string text = additionalFile.GetText()?.ToString();
                    if (!DocumentMetadata.TryCreateFromXml(text, errorCollector, out var metadata))
                    {
                        continue;
                    }

                    metadata.Validate(errorCollector);

                    SchemaElement element = this.ProcessFile(additionalFile.Path, errorCollector, out string rootNamespace);

                    if (!string.IsNullOrEmpty(metadata.OutputXmlFileName))
                    {
                        string relativePath = Path.Combine(
                            Path.GetDirectoryName(additionalFile.Path),
                            metadata.OutputXmlFileName);

                        if (!errorCollector.HasErrors)
                        {
                            element.XmlElement.Save(relativePath);
                        }
                    }

                    if (metadata.GenerateCSharp == true)
                    {
                        var visitor = new SchemaVisitor(rootNamespace);

                        element.Accept(visitor);
                        visitor.Finish();

                        if (!errorCollector.HasErrors)
                        {
                            string cSharp = visitor.StringBuilder.ToString();
                            context.AddSource(
                                Path.GetFileNameWithoutExtension(additionalFile.Path),
                                cSharp);
                        }
                    }
                }
            }
            catch (BailOutException)
            {
            }
#if DEBUG
            catch
            {
                Debugger.Launch();
            }
#endif
        }
    }

    private SchemaElement ProcessFile(string path, IErrorCollector errorCollector, out string rootNamespace)
    {
        rootNamespace = string.Empty;

        SchemaElement? baseSchema = null;

        string text = File.ReadAllText(path);
        if (DocumentMetadata.TryCreateFromXml(text, errorCollector, out DocumentMetadata? metadata))
        {
            if (!string.IsNullOrEmpty(metadata.BaseFileName))
            {
                string baseFilePath = Path.IsPathRooted(metadata.BaseFileName)
                    ? metadata.BaseFileName
                    : Path.Combine(Path.GetDirectoryName(path), metadata.BaseFileName);

                baseSchema = this.ProcessFile(baseFilePath, errorCollector, out rootNamespace);
            }
        }

        XDocument document = XDocument.Parse(text);

        if (baseSchema is not null)
        {
            if (!baseSchema.MergeWith(document.Root, errorCollector))
            {
                throw new BailOutException();
            }
        }
        else
        {
            rootNamespace = metadata?.NamespaceName ?? "";

            if (!SchemaParser.TryParse(document, errorCollector, out baseSchema))
            {
                throw new BailOutException();
            }
        }

        return baseSchema;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    private class ErrorCollector : IErrorCollector
    {
        private readonly GeneratorExecutionContext context;
        private readonly string file;

        public ErrorCollector(ref GeneratorExecutionContext context, string file)
        {
            this.context = context;
            this.file = file;
        }

        public bool HasErrors { get; private set; }

        public void Error(string message, string? path = null)
        {
            this.HasErrors = true;

            this.context.ReportDiagnostic(Diagnostic.Create(
                "CDN0001",
                "ConfiguratorDotNet",
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
                "CDN0001",
                "ConfiguratorDotNet",
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
                "CDN0002",
                "ConfiguratorDotNet",
                $"Message = '{message}' Path = '{path}'",
                severity: DiagnosticSeverity.Warning,
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                warningLevel: 1,
                isSuppressed: false,
                location: Location.Create(this.file, default, default)));
        }
    }

    private class BailOutException : Exception
    {
    }
}
