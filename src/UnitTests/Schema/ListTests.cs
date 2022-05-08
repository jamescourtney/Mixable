namespace UnitTests;

public class ListTests
{
    private const string BaseXml =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata>
        <mx:NamespaceName>Foo.Bar.Baz</mx:NamespaceName>
    </mx:Metadata>

    <List>
        <mx:ListItemTemplate>
            <Item>
                <A>5</A>
                <B>false</B>
                <C mx:Optional=""true"">string</C>
            </Item>
        </mx:ListItemTemplate>
        
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
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />

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
        parser.Parse(root, new BaseSchemaAttributeValidator(), tec, n => new ScalarSchemaElement(ScalarType.String, n));
        Assert.Single(tec.Errors);
        Assert.Equal(("Expected tag name: 'Item1'. Got: 'Item2'.", "/Configuration/List/Item2"), tec.Errors[0]);

        tec.Reset();
        SchemaParser wholeParser = new(tec);

        Assert.False(wholeParser.TryParse(XDocument.Parse(xml), out _));
        Assert.Single(tec.Errors);
        Assert.Equal(("No ISchemaElementParser was able to parse the given node.", "/Configuration/List"), tec.Errors[0]);
    }

    [Fact]
    public void Parse_InvalidListSchema_MultipleTemplateNodes()
    {
        string xml =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />

    <List>
        <mx:ListItemTemplate>
            <Item>4</Item>
        </mx:ListItemTemplate>
        <mx:ListItemTemplate>
            <Item>4</Item>
        </mx:ListItemTemplate>
    </List>
</Configuration>
";
        TestErrorCollector tec = new();

        ListSchemaElementParser parser = new ListSchemaElementParser();

        var root = XDocument.Parse(xml).XPathSelectElement("/Configuration/List");

        Assert.True(parser.CanParse(root, default));
        parser.Parse(root, new BaseSchemaAttributeValidator(), tec, n => new ScalarSchemaElement(ScalarType.String, n));
        Assert.Single(tec.Errors, ("Lists may only have a single template node.", "/Configuration/List"));

        tec.Reset();
        SchemaParser wholeParser = new(tec);
        Assert.False(wholeParser.TryParse(XDocument.Parse(xml), out _));
        Assert.Single(tec.Errors, ("Lists may only have a single template node.", "/Configuration/List"));
    }

    [Fact]
    public void Parse_InvalidListSchema_EmptyTemplateNode()
    {
        string xml =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />

    <List>
        <mx:ListItemTemplate />
    </List>
</Configuration>
";
        TestErrorCollector tec = new();

        SchemaParser wholeParser = new(tec);
        Assert.False(wholeParser.TryParse(XDocument.Parse(xml), out _));
        Assert.Single(tec.Errors, ("List templates must have exactly one child element.", "/Configuration/List"));
    }

    [Fact]
    public void Parse_InvalidListSchema_InvalidTemplateNode()
    {
        string xml =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />

    <List>
        <mx:ListItemTemplate>
            <Item1>4</Item1>
            <Item2>4</Item2>
        </mx:ListItemTemplate>
    </List>
</Configuration>
";
        TestErrorCollector tec = new();

        SchemaParser wholeParser = new(tec);
        Assert.False(wholeParser.TryParse(XDocument.Parse(xml), out _));
        Assert.Single(tec.Errors, ("List templates must have exactly one child element.", "/Configuration/List"));
    }

    [Fact]
    public void Merge_List_InvalidMergeStrategy()
    {
        string overrideSchema =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <List mx:ListMerge=""Invalid"" />
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
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
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

    [Fact]
    public void List_With_Explicit_Tag_No_Template()
    {
        string schema =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata>
        <mx:NamespaceName>Foo.Bar.Baz</mx:NamespaceName>
    </mx:Metadata>
    <List mx:List=""true"" />
</Configuration>
";

        XDocument doc = XDocument.Parse(schema);
        XElement element = doc.XPathSelectElement("/Configuration/List");

        ListSchemaElementParser parser = new();
        Assert.True(parser.CanParse(element, MetadataAttributes.Extract(element, null)));

        TestErrorCollector tec = new();
        Assert.False(new SchemaParser(tec).TryParse(doc, out SchemaElement? root));

        Assert.Single(
            tec.Errors,
            ("Couldn't determine type of list item. Lists must include at least one represenative node or a Template element.", "/Configuration/List"));
    }

    [Fact]
    public void List_With_Explicit_Tag()
    {
        string schema =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata>
        <mx:NamespaceName>Foo.Bar.Baz</mx:NamespaceName>
    </mx:Metadata>
    <List mx:List=""true"">
        <Item>
            <A>6</A>
            <C>foobar</C>
        </Item>
    </List>
</Configuration>
";

        XDocument doc = XDocument.Parse(schema);
        XElement element = doc.XPathSelectElement("/Configuration/List");

        ListSchemaElementParser parser = new();
        Assert.True(parser.CanParse(element, MetadataAttributes.Extract(element, null)));

        TestErrorCollector tec = new();

        Assert.True(new SchemaParser(tec).TryParse(doc, out SchemaElement? root));
        Assert.NotNull(root);

        var map = Assert.IsType<MapSchemaElement>(root);
        var list = Assert.IsType<ListSchemaElement>(map.Children.Single().Value);
        var template = Assert.IsType<MapSchemaElement>(list.Template);

        Assert.Equal(
            Assert.IsType<ScalarSchemaElement>(template.Children[XName.Get("A")]).ScalarType,
            ScalarType.Int);

        Assert.Equal(
            Assert.IsType<ScalarSchemaElement>(template.Children[XName.Get("C")]).ScalarType,
            ScalarType.String);
    }
}