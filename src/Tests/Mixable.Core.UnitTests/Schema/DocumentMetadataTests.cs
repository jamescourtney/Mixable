namespace UnitTests;

public class DocumentMetadataTests
{
    [Fact]
    public void PythonMetdata()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata>
        <Python>
            <Enabled>true</Enabled>
            <OutputFile>Foo.py</OutputFile>
        </Python>
    </mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.True(DocumentMetadata.TryCreateFromXml(xml, tec, out var meta));
        Assert.Empty(tec.Errors);

        Assert.NotNull(meta.Python);
        Assert.True(meta.Python.Enabled);
        Assert.EndsWith("Foo.py", meta.Python.OutputFile);
        Assert.True(meta.HasCodeGenComponent());
    }

    [Fact]
    public void PythonMetdata_Disabled()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata>
        <Python>
            <Enabled>false</Enabled>
        </Python>
    </mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.True(DocumentMetadata.TryCreateFromXml(xml, tec, out var meta));
        Assert.Empty(tec.Errors);
        Assert.NotNull(meta.Python);
        Assert.False(meta.Python.Enabled);
        Assert.Null(meta.Python.OutputFile);
        Assert.False(meta.HasCodeGenComponent());
    }

    [Fact]
    public void PythonMetdata_Implicit_Disabled()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata>
        <Python />
    </mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.True(DocumentMetadata.TryCreateFromXml(xml, tec, out var meta));
        Assert.Empty(tec.Errors);
        Assert.NotNull(meta.Python);
        Assert.False(meta.Python.Enabled);
        Assert.Null(meta.Python.OutputFile);
        Assert.False(meta.HasCodeGenComponent());
    }

    [Fact]
    public void PythonMetdata_InvalidBool()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata>
        <Python>
            <Enabled>banana</Enabled>
        </Python>
    </mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.True(DocumentMetadata.TryCreateFromXml(xml, tec, out _));
        Assert.Single(tec.Errors, ("Unable to parse 'banana' as a boolean value.", "/Settings/Metadata/Python/Enabled"));
    }

    [Fact]
    public void PythonMetdata_MissingOutputFile()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata>
        <Python>
            <Enabled>true</Enabled>
        </Python>
    </mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.True(DocumentMetadata.TryCreateFromXml(xml, tec, out _));
        Assert.Single(tec.Errors, ("Python CodeGen must include the 'OutputFile' value when 'Enabled' is true.", "/Settings/Metadata/Python"));
    }

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
        <CSharp>
            <Enabled>true</Enabled>
        </CSharp>
    </mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.True(DocumentMetadata.TryCreateFromXml(xml, tec, out _));
        Assert.Single(tec.Errors, ("CSharp CodeGen must include the 'NamespaceName' value when 'Enabled' is true.", "/Settings/Metadata/CSharp"));
    }

    [Fact]
    public void MetadataWithCodeGen_WithBaseFile()
    {
        string xml = @$"
<Settings xmlns:mx=""{Constants.XMLNamespace}"">
    <mx:Metadata>
        <BaseFile>Something.mxml</BaseFile>
        <CSharp>
            <NamespaceName>foo</NamespaceName>
            <Enabled>true</Enabled>
        </CSharp>
    </mx:Metadata>
</Settings>";

        TestErrorCollector tec = new();
        Assert.True(DocumentMetadata.TryCreateFromXml(xml, tec, out _));
        Assert.Single(tec.Errors, ("'BaseFile' metadata should not be specified when CodeGen is enabled.", "/Settings/Metadata"));
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