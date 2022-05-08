namespace UnitTests;

public class MapTests
{
    private const string BaseXml =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <cdn:Metadata>
        <cdn:NamespaceName>Foo.Bar.Baz</cdn:NamespaceName>
    </cdn:Metadata>

    <A>4</A>
    <B>string</B>
    <C>
        <C>2.0</C>
    </C>
    <D>true</D>
    <E cdn:Type=""string"">false</E>
</Configuration>
";

    [Fact]
    public void Parse_InvalidBaseSchema_DuplicateTags()
    {
        string xml =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <cdn:Metadata />
    <A>4</A>
    <B>string</B>
    <A>3</A>
</Configuration>
";
        TestErrorCollector tec = new();

        MapSchemaElementParser parser = new MapSchemaElementParser();
        Assert.False(parser.CanParse(XDocument.Parse(xml).Root, default));
        parser.Parse(XDocument.Parse(xml).Root, new BaseSchemaAttributeValidator(), tec, n => new ScalarSchemaElement(ScalarType.String, n));
        Assert.Single(tec.Errors);
        Assert.Equal(("Duplicate tag name under map element", "/Configuration/A"), tec.Errors[0]);

        tec.Reset();
        SchemaParser wholeParser = new(tec);

        Assert.False(wholeParser.TryParse(XDocument.Parse(xml), out _));
        Assert.Single(tec.Errors);
        Assert.Equal(("No ISchemaElementParser was able to parse the given node.", "/Configuration"), tec.Errors[0]);
    }

    [Fact]
    public void Merge_Add_Derived_Key_Not_Allowed()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <SomethingElse>Foo</SomethingElse>
</Configuration>
";
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
            overrideSchema,
            "Merged schema contains key not present in base schema. Merging may not add new keys.",
            "/Configuration/SomethingElse");
    }

    [Fact]
    public void Merge_Duplicate_Derived_Keys_Not_Allowed()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <A>3</A>
    <A>5</A>
</Configuration>
";
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
            overrideSchema,
            "Duplicate tag detected in map element",
            "/Configuration/A");
    }
}