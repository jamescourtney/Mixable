namespace UnitTests;

public class MapTests
{
    private const string BaseXml =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <cdn:Metadata>
        <cdn:NamespaceName>Foo.Bar.Baz</cdn:NamespaceName>
    </cdn:Metadata>

    <Mapping>
        <A>4</A>
        <B>string</B>
        <C>
            <C>2.0</C>
        </C>
        <D>true</D>
        <E cdn:Type=""string"">false</E>
    </Mapping>
</Configuration>
";

    [Fact]
    public void Map_InvalidDefinition_DuplicateTags()
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
        parser.Parse(null, XDocument.Parse(xml).Root, new BaseSchemaAttributeValidator(), tec, (a, b) => new ScalarSchemaElement(ScalarType.String, a, b));
        Assert.Single(tec.Errors);
        Assert.Equal(("Duplicate tag name under map element", "/Configuration/A"), tec.Errors[0]);

        tec.Reset();
        SchemaParser wholeParser = new(tec);

        Assert.False(wholeParser.TryParse(XDocument.Parse(xml), out _));
        Assert.Single(tec.Errors);
        Assert.Equal(("No ISchemaElementParser was able to parse the given node.", "/Configuration"), tec.Errors[0]);
    }
}