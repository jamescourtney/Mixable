namespace UnitTests;

public class ScalarTests
{
    [Fact]
    public void Parse_InvalidExplicitType()
    {
        XDocument doc = CreateXml("4", "long");
        XElement element = doc.XPathSelectElement("/Configuration/Scalar");

        ScalarSchemaElementParser parser = new();
        Assert.True(parser.CanParse(element, MetadataAttributes.Extract(element, null)));

        TestErrorCollector tec = new();
        SchemaParser docParser = new(tec);
        Assert.False(docParser.TryParse(doc, out _));

        Assert.Single(
            tec.Errors,
            ("Mixable was unable to build a schema for the XML. This may be because the Mixable 'Type' attribute is set to 'long' and there is no handler registered for this type.", "/Configuration/Scalar"));
    }

    [Theory]
    [InlineData("foo", WellKnownType.Int)]
    [InlineData("foo", WellKnownType.Bool)]
    [InlineData("foo", WellKnownType.Double)]
    public void Parse_ValidExplicitType_ValueNotParseable(string value, WellKnownType explicitType)
    {
        XDocument doc = CreateXml(value, explicitType.ToString());
        XElement element = doc.XPathSelectElement("/Configuration/Scalar");

        ScalarSchemaElementParser parser = new();
        Assert.True(parser.CanParse(element, MetadataAttributes.Extract(element, null)));

        TestErrorCollector tec = new();
        parser.Parse(element, new BaseSchemaAttributeValidator(), tec, null);

        Assert.True(tec.HasErrors);
        Assert.Single(tec.Errors, ($"Unable to parse '{value}' as a '{explicitType}'.", "/Configuration/Scalar"));
    }

    [Theory]
    [InlineData(WellKnownType.Double, "2.0")]
    [InlineData(WellKnownType.Int, "2")]
    [InlineData(WellKnownType.String, "banana")]
    [InlineData(WellKnownType.Bool, "false")]
    [InlineData(WellKnownType.Bool, "true")]
    public void Parse_ValidInferredTypes(WellKnownType expectedType, string value)
    {
        XDocument doc = CreateXml(value, null);
        XElement element = doc.XPathSelectElement("/Configuration/Scalar");

        ScalarSchemaElementParser parser = new();
        Assert.True(parser.CanParse(element, MetadataAttributes.Extract(element, null)));

        TestErrorCollector tec = new();
        var parsed = Assert.IsType<ScalarSchemaElement>(parser.Parse(element, new BaseSchemaAttributeValidator(), tec, null));

        Assert.False(tec.HasErrors);
        Assert.Equal(expectedType, parsed.ScalarType.Type);
    }

    [Fact]
    public void Merge_AddChildToScalar()
    {
        XDocument @base = CreateXml("string", null);

        SchemaParser parser = new();
        Assert.True(parser.TryParse(@base, out var root));

        XDocument @override = XDocument.Parse($@"<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable""><mx:Metadata /><Scalar><Foo>4</Foo></Scalar></Configuration>");

        TestErrorCollector tec = new();
        Assert.False(root.MergeWith(@override.Root, tec, new IntermediateSchemaAttributeValidator()));
        Assert.Single(tec.Errors, ("Override schemas may not introduce children to scalar nodes.", "/Configuration/Scalar"));

        tec.Reset();
        Assert.False(root.MergeWith(@override.Root, tec, new LeafSchemaAttributeValidator()));
        Assert.Single(tec.Errors, ("Override schemas may not introduce children to scalar nodes.", "/Configuration/Scalar"));
    }

    private static XDocument CreateXml(string value, string? explicitType)
    {
        string explicitTypeAttribute = string.Empty;
        if (!string.IsNullOrEmpty(explicitType))
        {
            explicitTypeAttribute = $"mx:Type=\"{explicitType}\"";
        }

        return XDocument.Parse(
            $@"
            <Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
                <mx:Metadata>
                    <mx:NamespaceName>Foo.Bar.Baz</mx:NamespaceName>
                </mx:Metadata>
                <Scalar {explicitTypeAttribute}>{value}</Scalar>
            </Configuration>
            ");
    }
}
