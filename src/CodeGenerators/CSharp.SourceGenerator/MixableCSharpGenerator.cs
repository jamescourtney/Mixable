using System.IO;
using Microsoft.CodeAnalysis;
using Mixable.Schema;

namespace Mixable.SourceGenerator;

[Generator]
public class MixableCSharpGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Debugger.Launch();

        foreach (var additionalFile in context.AdditionalFiles)
        {
            try
            {
                if (additionalFile.Path.ToLowerInvariant().EndsWith(".mxml"))
                {
                    IErrorCollector errorCollector = new DeduplicatingErrorCollector(
                        new SourceGeneratorErrorCollector(ref context, additionalFile.Path));

                    (XDocument doc, DocumentMetadata metadata) = LoadMetadata(additionalFile.Path, errorCollector);

                    bool emitXml = !string.IsNullOrEmpty(metadata.OutputXmlFileName);
                    bool emitCSharp = metadata.GenerateCSharp == true;

                    if (!emitXml && !emitCSharp)
                    {
                        continue;
                    }

                    SchemaElement element = this.ProcessFile(
                        additionalFile.Path,
                        errorCollector,
                        new(),
                        0,
                        out string rootNamespace);

                    if (errorCollector.HasErrors)
                    {
                        continue;
                    }

                    if (emitXml)
                    {
                        string relativePath = Path.Combine(
                            Path.GetDirectoryName(additionalFile.Path),
                            metadata.OutputXmlFileName);

                        string text = element.XmlElement.ToString();
                        File.WriteAllText(relativePath, text);
                    }

                    if (emitCSharp)
                    {
                        var visitor = new SchemaVisitor(rootNamespace);
                        element.Accept(visitor);
                        visitor.Finish();

                        string cSharp = visitor.StringBuilder.ToString();
                        context.AddSource(
                            Path.GetFileNameWithoutExtension(additionalFile.Path),
                            cSharp);
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

    private static (XDocument doc, DocumentMetadata meta) LoadMetadata(string path, IErrorCollector errorCollector)
    {
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

        if (!DocumentMetadata.TryCreateFromXDocument(document, errorCollector, out DocumentMetadata? metadata))
        {
            throw new BailOutException();
        }

        return (document, metadata);
    }

    private SchemaElement ProcessFile(
        string path,
        IErrorCollector errorCollector,
        HashSet<string> visitedPaths,
        uint depth,
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

            baseSchema = this.ProcessFile(baseFilePath, errorCollector, visitedPaths, depth + 1, out rootNamespace);
        }

        if (baseSchema is not null)
        {
            IAttributeValidator validator = depth switch
            {
                  0 => new LeafSchemaAttributeValidator(),
                > 0 => new IntermediateSchemaAttributeValidator(),
            };

            // Merge the contents here on top of the base.
            if (!baseSchema.MergeWith(document.Root, allowAbstract: depth > 0, errorCollector, validator))
            {
                throw new BailOutException();
            }
        }
        else
        {
            // This is the bottom of the stack -- parse the base schema to build a
            // structure.
            rootNamespace = metadata.NamespaceName ?? "Mixable.Generated";

            SchemaParser parser = new SchemaParser(errorCollector);

            if (!parser.TryParse(document, out baseSchema))
            {
                throw new BailOutException();
            }
        }

        return baseSchema;
    }


    /// <summary>
    /// Special exception to bail out after encountering an unrecoverable error.
    /// </summary>
    private class BailOutException : Exception
    {
    }
}
