using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Mixable.Schema;

/// <summary>
/// Describes metadata about a document and how Mixable should process it.
/// </summary>
public class DocumentMetadata
{
    [XmlElement("BaseFileName", Namespace = Constants.XMLNamespace)]
    public string? BaseFileName { get; set; }

    [XmlElement("Debug", Namespace = Constants.XMLNamespace)]
    public bool DebugBreak { get; set; }

    [XmlElement("CSharp", Namespace = Constants.XMLNamespace)]
    public Metadata.CSharpMetadata? CSharp { get; set; }

    [XmlElement("Python", Namespace = Constants.XMLNamespace)]
    public Metadata.PythonMetadata? Python { get; set; }

    [XmlElement("MergedXmlFileName", Namespace = Constants.XMLNamespace)]
    public string? MergedXmlFileName { get; set; }

    public static bool TryCreateFromFile(
        string filePath,
        IErrorCollector errorCollector,
        [NotNullWhen(true)] out DocumentMetadata? metadata)
    {
        XmlDocument doc = null;

        doc.DocumentElement.deser
        try
        {
            return TryCreateFromXml(File.ReadAllText(filePath), errorCollector, out metadata);
        }
        catch
        {
            errorCollector.Error("Unable to open file: " + filePath);
            metadata = null;
            return false;
        }
    }

    public static bool TryCreateFromXml(
        string xml,
        IErrorCollector errorCollector,
        [NotNullWhen(true)] out DocumentMetadata? metadata)
    {
        using StringReader reader = new StringReader(xml);

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Wrapper));
            var wrapper = (Wrapper?)serializer.Deserialize(reader);

            metadata = wrapper?.Metadata;
            if (metadata is null)
            {
                errorCollector.Error("Unable to find Mixable metadata node. The metadata node is required.");
                return false;
            }

            return true;
        }
        catch
        {
            errorCollector.Error("Unable to parse XML document");
            metadata = null;
            return false;
        }
    }

    public bool HasWork()
    {
        bool hasCodeGen = this.ContainsCodeGenTarget();
        bool hasXmlOutput = !string.IsNullOrEmpty(this.MergedXmlFileName);

        return hasCodeGen || hasXmlOutput;
    }

    public bool ContainsCodeGenTarget()
    {
        return this.CSharp is not null
            || this.Python is not null;
    }

    public class Wrapper
    {
        [XmlElement("Metadata", Namespace = Constants.XMLNamespace)]
        public DocumentMetadata? Metadata { get; set; }
    }
}
