using System.Xml.Serialization;

namespace Mixable.Schema.Metadata;

public class CSharpMetadata
{
    [XmlElement("NamespaceName", Namespace = Constants.XMLNamespace)]
    public string? NamespaceName { get; set; } = "Mixable.GeneratedCode";

    [XmlElement("OutputFile", Namespace = Constants.XMLNamespace)]
    public string? OutputFile { get; set; }

    [XmlElement("Enabled", Namespace = Constants.XMLNamespace)]
    public bool Enabled { get; set; }
}