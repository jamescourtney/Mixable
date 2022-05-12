using System.Xml.XPath;

namespace Mixable.Schema.Metadata;

public class CSharpMetadata
{
    public string? NamespaceName { get; init; }

    public bool Enabled { get; init; }

    public string? OutputFilePath { get; init; }

    public static CSharpMetadata? Parse(string documentPath, XElement metadataNode, IErrorCollector errorCollector)
    {
        var element = metadataNode.XPathSelectElement("CSharp");

        if (element is null)
        {
            return null;
        }

        var metadata = new CSharpMetadata
        {
            Enabled = element.ParseOptionalBool("Enabled", errorCollector, false),
            NamespaceName = element.ParseOptionalString("NamespaceName", null),
            OutputFilePath = element.ParseFilePath(documentPath, "OutputFile"),
        };

        if (metadata.Enabled && string.IsNullOrEmpty(metadata.NamespaceName))
        {
            errorCollector.Error("CSharp CodeGen must include the 'NamespaceName' value when 'Enabled' is true.", element.GetLocalDocumentPath());
        }

        return metadata;
    }
}