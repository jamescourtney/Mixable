namespace UnitTests;

public class ScalarTests
{
    [Fact]
    public void Parse_InvalidExplicitType()
    {
        XDocument doc = CreateXml("4", "long");
        XElement element = doc.XPathSelectElement("/Configuration/Scalar");

        ScalarSchemaElementParser parser = new();
        Assert.False(parser.CanParse(element, MetadataAttributes.Extract(element, null)));

        TestErrorCollector tec = new();
        parser.Parse(null, element, new BaseSchemaAttributeValidator(), tec, null);

        Assert.True(tec.HasErrors);
        Assert.Single(tec.Errors, ("Unable to find explicit scalar type 'long'.", "/Configuration/Scalar"));
    }

    [Fact]
    public void Parse_ValidExplicitType_ValueNotParseable()
    {
        XDocument doc = CreateXml("foo", "int");
        XElement element = doc.XPathSelectElement("/Configuration/Scalar");

        ScalarSchemaElementParser parser = new();
        Assert.True(parser.CanParse(element, MetadataAttributes.Extract(element, null)));

        TestErrorCollector tec = new();
        parser.Parse(null, element, new BaseSchemaAttributeValidator(), tec, null);

        Assert.True(tec.HasErrors);
        Assert.Single(tec.Errors, ("Unable to parse 'foo' as a 'int'.", "/Configuration/Scalar"));
    }

    [Theory]
    [InlineData("double", "2.0")]
    [InlineData("int", "2")]
    [InlineData("string", "banana")]
    [InlineData("bool", "false")]
    [InlineData("bool", "true")]
    public void Parse_ValidInferredTypes(string expectedType, string value)
    {
        XDocument doc = CreateXml(value, null);
        XElement element = doc.XPathSelectElement("/Configuration/Scalar");

        ScalarSchemaElementParser parser = new();
        Assert.True(parser.CanParse(element, MetadataAttributes.Extract(element, null)));

        TestErrorCollector tec = new();
        var parsed = Assert.IsType<ScalarSchemaElement>(parser.Parse(null, element, new BaseSchemaAttributeValidator(), tec, null));

        Assert.False(tec.HasErrors);
        Assert.Equal(expectedType, parsed.ScalarType.TypeName);
    }

    private static XDocument CreateXml(string value, string? explicitType)
    {
        string explicitTypeAttribute = string.Empty;
        if (!string.IsNullOrEmpty(explicitType))
        {
            explicitTypeAttribute = $"cdn:Type=\"{explicitType}\"";
        }

        return XDocument.Parse(
            $@"
            <Configuration xmlns:cdn=""http://configurator.net"">
                <cdn:Metadata>
                    <cdn:NamespaceName>Foo.Bar.Baz</cdn:NamespaceName>
                </cdn:Metadata>
                <Scalar {explicitTypeAttribute}>{value}</Scalar>
            </Configuration>
            ");
    }
}
