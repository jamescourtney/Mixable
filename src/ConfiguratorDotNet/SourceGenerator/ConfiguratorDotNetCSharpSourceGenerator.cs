using System.IO;
using Microsoft.CodeAnalysis;
using ConfiguratorDotNet.Schema;

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
                    SourceGeneratorErrorCollector errorCollector = new SourceGeneratorErrorCollector(
                        ref context,
                        additionalFile.Path);

                    (SchemaElement element, DocumentMetadata metadata) = this.ProcessFile(
                        additionalFile.Path,
                        errorCollector,
                        new(),
                        out string rootNamespace);

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
                // Debugger.Launch();
            }
#endif
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No-op
    }

    private (SchemaElement element, DocumentMetadata metadata) ProcessFile(
        string path,
        IErrorCollector errorCollector,
        HashSet<string> visitedPaths,
        out string rootNamespace)
    {
        // 1) Load the document, bailing if necessary.
        XDocument document;
        try
        {
            document = XDocument.Parse(File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            errorCollector.Error(ex.ToString(), path);
            throw new BailOutException();
        }

        // 2) Check for cycles.
        // TODO: Use hash or something more deterministic? Symlinks and whatnot
        // may be problematic if we're just using the literal path.
        if (!visitedPaths.Add(path))
        {
            errorCollector.Error("Cycle detected in include files.", path);
            throw new BailOutException();
        }

        rootNamespace = string.Empty;
        SchemaElement? baseSchema = null;

        // Load metadata -- bail if we fail.
        if (!DocumentMetadata.TryCreateFromXDocument(document, errorCollector, out DocumentMetadata? metadata))
        {
            throw new BailOutException();
        }

        // If this XML file inherits from a base file, load that one next.
        if (!string.IsNullOrEmpty(metadata.BaseFileName))
        {
            string baseFilePath = Path.IsPathRooted(metadata.BaseFileName)
                ? metadata.BaseFileName
                : Path.Combine(Path.GetDirectoryName(path), metadata.BaseFileName);

            (baseSchema, _) = this.ProcessFile(baseFilePath, errorCollector, visitedPaths, out rootNamespace);
        }

        if (baseSchema is not null)
        {
            // Merge the contents here on top of the base.
            if (!baseSchema.MergeWith(document.Root, errorCollector))
            {
                throw new BailOutException();
            }
        }
        else
        {
            // This is the bottom of the stack -- parse the base schema to build a
            // structure.
            rootNamespace = metadata.NamespaceName ?? "ConfiguratorDotNet.Generated";

            if (!SchemaParser.TryParse(document, errorCollector, out baseSchema))
            {
                throw new BailOutException();
            }
        }

        return (baseSchema, metadata);
    }


    /// <summary>
    /// Special exception to bail out after encountering an unrecoverable error.
    /// </summary>
    private class BailOutException : Exception
    {
    }
}
