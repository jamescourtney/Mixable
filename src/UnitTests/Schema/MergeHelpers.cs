using ConfiguratorDotNet.Schema;
using System.Xml.Linq;
using Xunit;
using System.Collections.Generic;

namespace UnitTests;

public class MergeHelpers
{
    public static void MergeInvalidSchema(
        string baseXml,
        string overrideXml,
        string expectedError,
        string expectedPath)
    {
        TestErrorCollector tec = new();

        SchemaParser parser = new SchemaParser(tec);
        Assert.True(parser.TryParse(XDocument.Parse(baseXml), out var result));

        XElement @override = XDocument.Parse(overrideXml).Root!;
        Assert.NotNull(@override);
        Assert.False(result.MergeWith(@override, tec));

        Assert.Contains(tec.Errors, x => x.path == expectedPath && x.msg == expectedError);
    }

    public static void Merge(
        string baseXml,
        string overrideXml,
        string expectedXml)
    {
        var tec = new TestErrorCollector();

        SchemaParser parser = new SchemaParser(tec);
        Assert.True(parser.TryParse(XDocument.Parse(baseXml), out var result));

        XElement @override = XDocument.Parse(overrideXml).Root!;
        result!.MergeWith(@override, tec);

        string merged = result.XmlElement.ToString();

        Assert.Equal(expectedXml, merged);
        Assert.False(tec.HasErrors);
    }
}
