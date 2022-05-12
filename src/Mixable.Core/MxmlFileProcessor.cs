using System.IO;

namespace Mixable.Schema;

/// <summary>
/// Processes MXML files.
/// </summary>
public class MxmlFileProcessor
{
    private readonly IErrorCollector errorCollector;
    private readonly string filePath;

    public MxmlFileProcessor(string filePath, IErrorCollector collector)
    {
        this.errorCollector = new DeduplicatingErrorCollector(collector);
        this.filePath = filePath;
    }

    public void MergeXml()
    {
        try
        {
            (_, DocumentMetadata metadata) = LoadMetadata(this.filePath, errorCollector);

            // Nothing to do.
            if (string.IsNullOrEmpty(metadata.MergedXmlFileName))
            {
                return;
            }

            SchemaElement element = this.ProcessFile(
                this.filePath,
                new(),
                0);

            if (errorCollector.HasErrors)
            {
                return;
            }

            if (!string.IsNullOrEmpty(metadata.MergedXmlFileName))
            {
                string text = element.XmlElement.ToString();
                File.WriteAllText(metadata.MergedXmlFileName, text);
            }
        }
        catch (BailOutException)
        {
        }

        return;
    }

    /// <summary>
    /// Processes the visitors over the file in this processor.
    /// </summary>
    /// <param name="visitors">The visitors.</param>
    /// <returns>True if any visitor was invoked.</returns>
    public bool TryApplyVisitors(IEnumerable<ISchemaVisitor> visitors)
    {
        try
        {
            (_, DocumentMetadata metadata) = LoadMetadata(filePath, this.errorCollector);

            visitors = visitors.Where(v => v.ShouldProcess(metadata));
            if (!visitors.Any())
            {
                return false;
            }

            SchemaElement element = this.ProcessFile(
                this.filePath,
                new(),
                0);

            if (this.errorCollector.HasErrors)
            {
                return false;
            }

            foreach (var visitor in visitors)
            {
                visitor.Run(element, metadata, this.errorCollector);
            }

            return true;
        }
        catch (BailOutException)
        {
        }

        return false;
    }

    private static (XDocument doc, DocumentMetadata meta) LoadMetadata(
        string path,
        IErrorCollector errorCollector)
    {
        if (!DocumentMetadata.TryCreateFromFile(path, errorCollector, out var document, out var metadata))
        {
            throw new BailOutException();
        }

        return (document, metadata);
    }

    private SchemaElement ProcessFile(
        string path,
        HashSet<string> visitedPaths,
        uint depth)
    {
        (XDocument document, DocumentMetadata metadata) = LoadMetadata(path, this.errorCollector);

        // Check for cycles.
        // TODO: Use hash or something more deterministic? Symlinks and whatnot
        // may be problematic if we're just using the literal path.
        if (!visitedPaths.Add(path))
        {
            this.errorCollector.Error("Cycle detected in include files.", path);
            throw new BailOutException();
        }

        SchemaElement? baseSchema = null;

        // If this XML file inherits from a base file, load that one next.
        if (!string.IsNullOrEmpty(metadata.BaseFileName))
        {
            baseSchema = this.ProcessFile(metadata.BaseFileName, visitedPaths, depth + 1);
        }

        if (baseSchema is not null)
        {
            IAttributeValidator validator = depth switch
            {
                0 => new LeafSchemaAttributeValidator(),
                > 0 => new IntermediateSchemaAttributeValidator(),
            };

            // Merge the contents here on top of the base.
            if (!baseSchema.MergeWith(document.Root!, allowAbstract: depth > 0, this.errorCollector, validator))
            {
                throw new BailOutException();
            }
        }
        else
        {
            SchemaParser parser = new SchemaParser(this.errorCollector);

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

    private static string GetDirectoryName(string path)
    {
        string? result = Path.GetDirectoryName(path);

        MixableInternal.Assert(
            result is not null,
            "couldn't get directory name from path: " + path);

        return result;
    }
}