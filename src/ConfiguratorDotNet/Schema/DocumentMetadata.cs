namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Describes metadata about a document and how CDN should process it.
/// </summary>
public class DocumentMetadata
{
    private string? namespaceName;
    private string? baseFileName;
    private string? outputXmlName;
    private bool? generateCSharp;

    private DocumentMetadata(XElement metadataElement)
    {
        Dictionary<XName, string> children = metadataElement.GetChildren().ToDictionary(x => x.Name, x => x.Value);

        children.TryGetValue(Constants.Tags.NamespaceTagName, out this.namespaceName);
        children.TryGetValue(Constants.Tags.OutputXmlFileTagName, out this.outputXmlName);
        children.TryGetValue(Constants.Tags.BaseFileName, out this.baseFileName);

        if (children.TryGetValue(Constants.Tags.GenerateCSharptagName, out string? generateCSharp))
        {
            this.generateCSharp = generateCSharp.Trim().ToLowerInvariant() switch
            {
                "true" => true,
                "false" => false,
                _ => null,
            };
        }
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

        return TryCreateFromXDocument(document, errorCollector, out metadata);
    }

    public static bool TryCreateFromXDocument(
        XDocument document,
        IErrorCollector errorCollector,
        [NotNullWhen(true)] out DocumentMetadata? metadata)
    {
        XElement? metadataElement = document
            .Root?
            .GetChildren()
            .SingleOrDefault(x => x.Name == Constants.Tags.RootTagName);

        if (metadataElement is null)
        {
            errorCollector.Error("Unable to find CDN metadata node.");
            metadata = null;
            return false;
        }

        metadata = new DocumentMetadata(document.Root!);
        metadata.Validate(errorCollector);

        return true;
    }

    public void Validate(IErrorCollector errorCollector)
    {
        if (this.GenerateCSharp == true)
        {
            if (string.IsNullOrEmpty(this.NamespaceName))
            {
                errorCollector.Warning("Namespace must be specified when 'GenerateCSharp' is true.");
            }

            if (!string.IsNullOrEmpty(this.BaseFileName))
            {
                errorCollector.Warning("BaseFileName should not be specified when GenerateCSharp is true.");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(this.NamespaceName))
            {
                errorCollector.Warning("Namespace Name may not be specified when GenerateCSharp is false.");
            }

            if (string.IsNullOrEmpty(this.BaseFileName))
            {
                errorCollector.Warning("BaseFileName should be specified when GenerateCSharp is false.");
            }
        }
    }

    public string? NamespaceName => this.namespaceName;

    public string? BaseFileName => this.baseFileName;

    public string? OutputXmlFileName => this.outputXmlName;

    public bool? GenerateCSharp => this.generateCSharp;
}
