using System.Xml.XPath;

namespace Mixable.Schema.Metadata;

public class PythonMetadata
{
    public string? OutputFile { get; init; }

    public bool Enabled { get; init; }

    public static PythonMetadata? Parse(string documentPath, XElement metadataNode, IErrorCollector errorCollector)
    {
        var element = metadataNode.XPathSelectElement("Python");

        if (element is null)
        {
            return null;
        }

        var md = new PythonMetadata
        {
            Enabled = element.ParseOptionalBool("Enabled", errorCollector, false),
            OutputFile = element.ParseFilePath(documentPath, "OutputFile"),
        };

        if (md.Enabled && string.IsNullOrEmpty(md.OutputFile))
        {
            errorCollector.Error("Python CodeGen must include the 'OutputFile' value when 'Enabled' is true.", element.GetLocalDocumentPath());
        }

        return md;
    }
}