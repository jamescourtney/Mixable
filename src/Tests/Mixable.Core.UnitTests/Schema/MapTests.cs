namespace UnitTests;

public class MapTests
{
    private const string BaseXml =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata>
        <mx:NamespaceName>Foo.Bar.Baz</mx:NamespaceName>
    </mx:Metadata>

    <A>4</A>
    <B>string</B>
    <C>
        <C>2.0</C>
    </C>
    <D>true</D>
    <E mx:Type=""string"">false</E>
    <F mx:Flags=""Final"">string</F>
    <G mx:Flags=""Abstract"">string</G>
</Configuration>
";

    [Fact]
    public void Parse_InvalidBaseSchema_DuplicateTags()
    {
        string xml =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />
    <A>4</A>
    <B>string</B>
    <A>3</A>
</Configuration>
";
        TestErrorCollector tec = new();

        MapSchemaElementParser parser = new MapSchemaElementParser();
        Assert.False(parser.CanParse(XDocument.Parse(xml).Root, default));
        parser.Parse(XDocument.Parse(xml).Root, new BaseSchemaAttributeValidator(), tec, (n, av) => new ScalarSchemaElement(ScalarType.String, n));
        Assert.Single(tec.Errors, ("Duplicate tag name under map element", "/Configuration/A"));

        tec.Reset();
        SchemaParser wholeParser = new(tec);

        Assert.False(wholeParser.TryParse(XDocument.Parse(xml), out _));
        Assert.Single(tec.Errors, ("Mixable was unable to build a schema for the XML. Consider adding the Mixable 'Type' attribute to tell Mixable how to interpet the schema. Suggestions are: List, Int, String, Double, Bool, Map", "/Configuration"));
    }

    [Fact]
    public void Merge_Add_Derived_Key_Not_Allowed()
    {
        string overrideSchema =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />
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
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />
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

    [Fact]
    public void Merge_Cannot_Override_Final()
    {
        string overrideSchema =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />
    <F>blah</F>
</Configuration>
";
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
            overrideSchema,
            "Nodes marked as 'Final' may not be overridden.",
            "/Configuration/F");
    }

    [Fact]
    public void Merge_Must_Override_Abstract()
    {
        string overrideSchema =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />
</Configuration>
";
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
            overrideSchema,
            "Abstract nodes are not permitted to remain after the final merge.",
            "/Configuration/G",
            isLeafDocument: true);
    }

    [Fact]
    public void Merge_Must_Override_Abstract_OK()
    {
        string overrideSchema =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />
    <G>foobar</G>
</Configuration>
";
        XDocument mergedDoc = MergeHelpers.Merge(
            BaseXml,
            overrideSchema,
            expectedXml: null,
            isLeafDocument: true);

        XElement element = mergedDoc.XPathSelectElement("/Configuration/G");
        Assert.NotNull(element);
        Assert.Equal("foobar", element.Value);
        Assert.Equal("None", element.Attribute(Constants.Attributes.Flags).Value);
    }

    [Theory]
    [InlineData(NodeModifier.Final)]
    [InlineData(NodeModifier.Abstract)]
    [InlineData(NodeModifier.Optional)]
    public void Merge_Add_Modifier_To_Map_Leaf_NotAllowed(NodeModifier modifier)
    {
        string overrideSchema =
$@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />
    <D mx:Flags=""{modifier}"">false</D>
    <G>foobar</G>
</Configuration>
";
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
            overrideSchema,
            $"Leaf schemas may not use the Mixable 'Flags' attribute.",
            "/Configuration/D",
            isLeafDocument: true);
    }


    [Theory]
    [InlineData(NodeModifier.Optional)]
    [InlineData((NodeModifier)255)]
    public void Merge_Add_Modifier_To_Map_Intermediate_NotAllowed(NodeModifier modifier)
    {
        string overrideSchema =
$@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />
    <D mx:Flags=""{modifier}"">false</D>
    <G>foobar</G>
</Configuration>
";
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
            overrideSchema,
            $"Intermediate schemas may not use the Flags attribute to set a node to {modifier}.",
            "/Configuration/D",
            isLeafDocument: false);
    }

    [Theory]
    [InlineData(NodeModifier.Abstract)]
    [InlineData(NodeModifier.Final)]
    [InlineData(NodeModifier.Optional)]
    public void Map_Template_Invalid_Flags(NodeModifier modifier)
    {
        string xml =
$@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata>
        <mx:NamespaceName>Foo.Bar.Baz</mx:NamespaceName>
    </mx:Metadata>
    <C mx:Flags=""{modifier}"">
        <C>2.0</C>
    </C>
</Configuration>
";

        TestErrorCollector tec = new();
        SchemaParser p = new(tec);
        Assert.False(p.TryParse(XDocument.Parse(xml), out var root));

        Assert.Contains(($"The Flags attribute value '{modifier}' is not valid on Map nodes", "/Configuration/C"), tec.Errors);
    }

    [Theory]
    [InlineData(NodeModifier.Abstract)]
    [InlineData(NodeModifier.Final)]
    [InlineData(NodeModifier.Optional)]
    public void Map_Derived_Invalid_Flags(NodeModifier modifier)
    {
        string xml =
$@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata />
    <C mx:Flags=""{modifier}"">
        <C>2.0</C>
    </C>
</Configuration>
";

        TestErrorCollector tec = new();
        SchemaParser p = new(tec);
        Assert.True(p.TryParse(XDocument.Parse(BaseXml), out var root));
        Assert.False(root.MergeWith(XDocument.Parse(xml).Root, allowAbstract: true, tec, new IntermediateSchemaAttributeValidator()));

        Assert.Contains(("Map elements in override schemas may not specify the 'Flags' attribute.", "/Configuration/C"), tec.Errors);
    }
}