namespace UnitTests;

public class MergeHelpers
{
    public static void MergeInvalidSchema(
        string baseXml,
        string overrideXml,
        string expectedError,
        string expectedPath,
        bool isLeafDocument = false)
    {
        IAttributeValidator validator = isLeafDocument
            ? new LeafSchemaAttributeValidator()
            : new IntermediateSchemaAttributeValidator();

        TestErrorCollector tec = new();

        SchemaParser parser = new SchemaParser(tec);
        Assert.True(parser.TryParse(XDocument.Parse(baseXml), out var result));

        XElement @override = XDocument.Parse(overrideXml).Root!;

        Assert.NotNull(@override);
        Assert.False(result.MergeWith(@override, allowAbstract: !isLeafDocument, tec, validator));
        Assert.Contains(tec.Errors, x => x.path == expectedPath && x.msg == expectedError);
    }

    public static XDocument Merge(
        string baseXml,
        string overrideXml,
        string? expectedXml,
        bool isLeafDocument = false)
    {
        IAttributeValidator validator = isLeafDocument
            ? new LeafSchemaAttributeValidator()
            : new IntermediateSchemaAttributeValidator();

        var tec = new TestErrorCollector();

        SchemaParser parser = new SchemaParser(tec);
        Assert.True(parser.TryParse(XDocument.Parse(baseXml), out var result));

        XElement @override = XDocument.Parse(overrideXml).Root!;
        result.MergeWith(@override, allowAbstract: !isLeafDocument, tec, validator);

        string merged = result.XmlElement.ToString();

        if (expectedXml is not null)
        {
            Assert.Equal(expectedXml, merged);
        }

        Assert.False(tec.HasErrors);

        return XDocument.Parse(merged);
    }
}
