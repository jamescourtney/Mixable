using System.Xml.XPath;

namespace UnitTests;

public class ListTests
{
    private const string BaseXml =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <cdn:Metadata>
        <cdn:NamespaceName>Foo.Bar.Baz</cdn:NamespaceName>
    </cdn:Metadata>

    <List>
        <cdn:ListItemTemplate>
            <Item>
                <A>5</A>
                <B>false</B>
                <C cdn:Optional=""true"">string</C>
            </Item>
        </cdn:ListItemTemplate>
        
        <Item>
            <A>4</A>
            <B>true</B>
        </Item>
    </List>
</Configuration>
";

    [Fact]
    public void Parse_InvalidBaseSchema_DuplicateTags()
    {
        string xml =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <cdn:Metadata />

    <List>
        <Item1>4</Item1>
        <Item2>5</Item2>
        <Item1>5</Item1>
    </List>
</Configuration>
";
        TestErrorCollector tec = new();

        ListSchemaElementParser parser = new ListSchemaElementParser();

        var root = XDocument.Parse(xml).XPathSelectElement("/Configuration/List");

        Assert.False(parser.CanParse(root, default));
        parser.Parse(null, root, new BaseSchemaAttributeValidator(), tec, (a, b) => new ScalarSchemaElement(ScalarType.String, a, b));
        Assert.Single(tec.Errors);
        Assert.Equal(("Expected tag name: 'Item1'. Got: 'Item2'.", "/Configuration/List/Item2"), tec.Errors[0]);

        tec.Reset();
        SchemaParser wholeParser = new(tec);

        Assert.False(wholeParser.TryParse(XDocument.Parse(xml), out _));
        Assert.Single(tec.Errors);
        Assert.Equal(("No ISchemaElementParser was able to parse the given node.", "/Configuration/List"), tec.Errors[0]);
    }

    [Fact]
    public void Merge_List_InvalidMergeStrategy()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <List cdn:ListMerge=""Invalid"" />
</Configuration>
";
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
            overrideSchema,
            "Unable to parse 'Invalid' as a list merge value. Valid values are: Concatenate,Replace.",
            "/Configuration/List");
    }

    [Fact]
    public void Merge_List_Child_Missing_Required_Field()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <List>
        <Item>
            <A>6</A>
            <C>foobar</C>
        </Item>
    </List>
</Configuration>
";
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
            overrideSchema,
            "Schema mismatch. Missing required children: B",
            "/Configuration/List/Item");
    }
}