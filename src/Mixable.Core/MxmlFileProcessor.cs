using System.IO;

namespace Mixable.Schema;

/// <summary>
/// Processes MXML files.
/// </summary>
public class MxmlFileProcessor
{
    /// <summary>
    /// Processes the given file.
    /// </summary>
    /// <param name="filePath">The absolute file path.</param>
    /// <param name="visitor">The visitor to apply to the schema.</param>
    /// <param name="errorCollector">The error collector for error aggregation.</param>
    /// <returns>True if the visitor was invoked.</returns>
    public bool TryProcessFile<T>(
        string filePath,
        ISchemaVisitor<T> visitor,
        IErrorCollector errorCollector,
        [NotNullWhen(true)] out DocumentMetadata? metadata)
    {
        metadata = null;

        try
        {
            errorCollector = new DeduplicatingErrorCollector(errorCollector);

            (_, metadata) = LoadMetadata(filePath, errorCollector);

            if (metadata.IsNoOp)
            {
                return false;
            }

            SchemaElement element = this.ProcessFile(
                filePath,
                errorCollector,
                new(),
                0);

            if (errorCollector.HasErrors)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(metadata.OutputXmlFileName))
            {
                string relativePath = Path.Combine(
                    GetDirectoryName(filePath),
                    metadata.OutputXmlFileName);

                string text = element.XmlElement.ToString();
                File.WriteAllText(relativePath, text);
            }

            visitor.Initialize(metadata);
            element.Accept(visitor);
            return true;
        }
        catch (BailOutException)
        {
            return false;
        }
    }

    private static (XDocument doc, DocumentMetadata meta) LoadMetadata(
        string path,
        IErrorCollector errorCollector)
    {
        XDocument document;
        try
        {
            document = XDocument.Parse(File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            errorCollector.Error(ex.Message, path);
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
        uint depth)
    {
        (XDocument document, DocumentMetadata metadata) = LoadMetadata(path, errorCollector);

        // Check for cycles.
        // TODO: Use hash or something more deterministic? Symlinks and whatnot
        // may be problematic if we're just using the literal path.
        if (!visitedPaths.Add(path))
        {
            errorCollector.Error("Cycle detected in include files.", path);
            throw new BailOutException();
        }

        SchemaElement? baseSchema = null;

        // If this XML file inherits from a base file, load that one next.
        if (!string.IsNullOrEmpty(metadata.BaseFileName))
        {
            string baseFilePath = Path.IsPathRooted(metadata.BaseFileName)
                ? metadata.BaseFileName
                : Path.Combine(GetDirectoryName(path), metadata.BaseFileName);

            baseSchema = this.ProcessFile(baseFilePath, errorCollector, visitedPaths, depth + 1);
        }

        if (baseSchema is not null)
        {
            IAttributeValidator validator = depth switch
            {
                0 => new LeafSchemaAttributeValidator(),
                > 0 => new IntermediateSchemaAttributeValidator(),
            };

            // Merge the contents here on top of the base.
            if (!baseSchema.MergeWith(document.Root!, allowAbstract: depth > 0, errorCollector, validator))
            {
                throw new BailOutException();
            }
        }
        else
        {
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

    private static string GetDirectoryName(string path)
    {
        string? result = Path.GetDirectoryName(path);

        MixableInternal.Assert(
            result is not null,
            "couldn't get directory name from path: " + path);

        return result;
    }
}