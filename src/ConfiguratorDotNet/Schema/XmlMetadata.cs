namespace ConfiguratorDotNet.Schema;

internal class XmlMetadata
{
    private string? namespaceName;
    private string? baseFileName;
    private string? outputXmlName;
    private bool? generateCSharp;

    public XmlMetadata(XElement root)
    {
        XElement? metadataElement = root
            .GetChildren()
            .SingleOrDefault(x => x.Name == Constants.Metadata.RootTagName);

        if (metadataElement is null)
        {
            throw new ConfiguratorDotNetException("Missing <Metadata> element.");
        }

        Dictionary<XName, string> children = metadataElement.GetChildren().ToDictionary(x => x.Name, x => x.Value);

        children.TryGetValue(Constants.Metadata.NamespaceTagName, out this.namespaceName);
        children.TryGetValue(Constants.Metadata.OutputXmlFileTagName, out this.outputXmlName);
        children.TryGetValue(Constants.Metadata.BaseFileName, out this.baseFileName);

        if (children.TryGetValue(Constants.Metadata.GenerateCSharptagName, out string? generateCSharp))
        {
            this.generateCSharp = generateCSharp.Trim().ToLowerInvariant() switch
            {
                "true" => true,
                "false" => false,
                _ => null,
            };
        }
    }

    public string? NamespaceName => this.namespaceName;

    public string? BaseFileName => this.baseFileName;

    public string? OutputXmlFileName => this.outputXmlName;

    public bool? GenerateCSharp => this.generateCSharp;
}
