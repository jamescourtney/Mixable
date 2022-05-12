namespace Mixable.Schema;

/// <summary>
/// Describes metadata about a document and how Mixable should process it.
/// </summary>
public class DocumentMetadata
{
    private DocumentMetadata(string documentPath, XElement metadataElement, IErrorCollector errorCollector)
    {
        this.MergedXmlFileName = Metadata.MetadataParseHelpers.ParseFilePath(metadataElement, documentPath, "MergedXmlFile");
        this.BaseFileName = Metadata.MetadataParseHelpers.ParseFilePath(metadataElement, documentPath, "BaseFile");
        this.CSharp = Metadata.CSharpMetadata.Parse(documentPath, metadataElement, errorCollector);
        this.Python = Metadata.PythonMetadata.Parse(documentPath, metadataElement, errorCollector);
    }

    public static bool TryCreateFromFile(
        string filePath,
        IErrorCollector errorCollector,
        [NotNullWhen(true)] out XDocument? document,
        [NotNullWhen(true)] out DocumentMetadata? metadata)
    {
        try
        {
            document = XDocument.Parse(System.IO.File.ReadAllText(filePath));
        }
        catch (Exception ex)
        {
            errorCollector.Error("Unable to parse XML document: " + ex.Message);
            metadata = null;
            document = null;

            return false;
        }

        return TryCreateFromXDocument(document, System.IO.Path.GetDirectoryName(filePath), errorCollector, out metadata);
    }

    public static bool TryCreateFromXml(
        string xml,
        IErrorCollector errorCollector,
        [NotNullWhen(true)] out DocumentMetadata? metadata)
    {
        XDocument document;
        try
        {
            document = XDocument.Parse(xml);
        }
        catch
        {
            errorCollector.Error("Unable to parse XML document");
            metadata = null;
            return false;
        }

        return TryCreateFromXDocument(document, null, errorCollector, out metadata);
    }

    public static bool TryCreateFromXDocument(
        XDocument document,
        string? documentPath,
        IErrorCollector errorCollector,
        [NotNullWhen(true)] out DocumentMetadata? metadata)
    {
        IEnumerable<XElement>? metadataElements = document
            .Root?
            .GetChildren(Constants.Tags.MetdataTagName);

        if (metadataElements is null || !metadataElements.Any())
        {
            errorCollector.Error("Unable to find Mixable metadata node. The metadata node is required.");
            metadata = null;
            return false;
        }

        if (metadataElements.Count() != 1)
        {
            errorCollector.Error("Only one Mixable metadata node may be specified.");
            metadata = null;
            return false;
        }

        documentPath ??= System.IO.Path.GetDirectoryName(typeof(DocumentMetadata).Assembly.Location)!;

        metadata = new DocumentMetadata(documentPath, metadataElements.First(), errorCollector);

        if (metadata.HasCodeGenComponent() && !string.IsNullOrEmpty(metadata.BaseFileName))
        {
            // some error about codegen and merging usually being different files.
            errorCollector.Error("'BaseFile' metadata should not be specified when CodeGen is enabled.", metadataElements.First().GetLocalDocumentPath());
        }

        return true;
    }

    public string? BaseFileName { get; }

    public string? MergedXmlFileName { get; }

    public Metadata.CSharpMetadata? CSharp { get; }

    public Metadata.PythonMetadata? Python { get; }

    public bool HasCodeGenComponent()
    {
        return this.CSharp?.Enabled == true
            || this.Python?.Enabled == true;
    }
}
