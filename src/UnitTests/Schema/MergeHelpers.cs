namespace UnitTests;

public class MergeHelpers
{
    public static void MergeInvalidSchema(
        string baseXml,
        string overrideXml,
        string expectedError,
        string expectedPath,
        IAttributeValidator? validator = null)
    {
        validator ??= new IntermediateSchemaAttributeValidator();

        TestErrorCollector tec = new();

        SchemaParser parser = new SchemaParser(tec);
        Assert.True(parser.TryParse(XDocument.Parse(baseXml), out var result));

        XElement @override = XDocument.Parse(overrideXml).Root!;
        Assert.NotNull(@override);
        Assert.False(result.MergeWith(@override, tec, validator));

        Assert.Contains(tec.Errors, x => x.path == expectedPath && x.msg == expectedError);
    }

    public static void Merge(
        string baseXml,
        string overrideXml,
        string expectedXml,
        IAttributeValidator? validator = null)
    {
        validator ??= new IntermediateSchemaAttributeValidator();

        var tec = new TestErrorCollector();

        SchemaParser parser = new SchemaParser(tec);
        Assert.True(parser.TryParse(XDocument.Parse(baseXml), out var result));

        XElement @override = XDocument.Parse(overrideXml).Root!;
        result!.MergeWith(@override, tec, validator);

        string merged = result.XmlElement.ToString();

        Assert.Equal(expectedXml, merged);
        Assert.False(tec.HasErrors);
    }
}
