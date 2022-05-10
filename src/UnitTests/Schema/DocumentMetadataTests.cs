namespace UnitTests;

public class DocumentMetadataTests
{
    [Fact]
    public void MetadataNodeMissing()
    {
        string xml = "<Settings />";
        TestErrorCollector tec = new();
        Assert.False(DocumentMetadata.TryCreateFromXml(xml, tec, out _));
        Assert.Single(tec.Errors, ("Unable to find Mixable metadata node. The metadata node is required.", (string)null));
    }

    [Fact]
    public void MetadataNodeDuplicate()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata />
    <mx:Metadata />
</Settings>";

        TestErrorCollector tec = new();
        Assert.False(DocumentMetadata.TryCreateFromXml(xml, tec, out _));
        Assert.Single(tec.Errors, ("Only one Mixable metadata node may be specified.", (string)null));
    }

    [Fact]
    public void MetadataWithCodeGen_NoNamespace()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata>
        <mx:GenerateCSharp>true</mx:GenerateCSharp>
    </mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.True(DocumentMetadata.TryCreateFromXml(xml, tec, out _));
        Assert.Single(tec.Errors, ("Namespace must be specified when 'GenerateCSharp' is true.", (string)null));
    }

    [Fact]
    public void MetadataWithCodeGen_WithBaseFile()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata>
        <mx:GenerateCSharp>true</mx:GenerateCSharp>
        <mx:NamespaceName>Foo.Bar</mx:NamespaceName>
        <mx:BaseFile>Something.mxml</mx:BaseFile>
    </mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.True(DocumentMetadata.TryCreateFromXml(xml, tec, out _));
        Assert.Single(tec.Errors, ("BaseFileName should not be specified when GenerateCSharp is true.", (string)null));
    }

    [Fact]
    public void Metadata_FromString_InvalidXml()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.False(DocumentMetadata.TryCreateFromXml(xml, tec, out _));
        Assert.Single(tec.Errors, ("Unable to parse XML document", (string)null));
    }
}