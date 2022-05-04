using ConfiguratorDotNet.Schema;
using System.Xml.Linq;
using Xunit;
using System.Collections.Generic;

namespace UnitTests;

public class MergeTests
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
    </Mapping>
    <List>
        <Item>1</Item>
        <Item>2</Item>
        <Item>3</Item>
    </List>
    <FancyList>
        <Item>
            <Value>1</Value>
        </Item>
        <Item>
            <Value>2</Value>
        </Item>
        <Item>
            <Value>3</Value>
        </Item>
    </FancyList>
</Configuration>
";

    [Fact]
    public void ValidMerge()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <cdn:Metadata>
        <cdn:BaseFile>base.xml</cdn:BaseFile>
    </cdn:Metadata>

    <List>
       <Item>4</Item>
    </List>

    <Mapping>
        <A>5</A>
    </Mapping>

    <FancyList cdn:ListMerge=""Replace"" />
</Configuration>
";
        string expected =
@"<Configuration xmlns:cdn=""http://configurator.net"">
  <cdn:Metadata>
    <cdn:NamespaceName>Foo.Bar.Baz</cdn:NamespaceName>
  </cdn:Metadata>
  <Mapping>
    <A>5</A>
    <B>string</B>
    <C>
      <C>2.0</C>
    </C>
  </Mapping>
  <List>
    <Item>1</Item>
    <Item>2</Item>
    <Item>3</Item>
    <Item>4</Item>
  </List>
  <FancyList />
</Configuration>";

        Merge(overrideSchema, expected);
    }

    [Fact]
    public void Scalar_Override_NotParseable()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <Mapping>
        <A>some string</A>
    </Mapping>
</Configuration>
";
        MergeInvalidSchema(
            overrideSchema,
            "Failed to parse 'some string' as a type of 'int'.",
            "/Configuration/Mapping/A");
    }

    [Fact]
    public void Scalar_Override_TypeChange()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <Mapping>
        <A cdn:Type=""bool"">true</A>
    </Mapping>
</Configuration>
";
        MergeInvalidSchema(
            overrideSchema,
            "Derived schemas may not have the Type attribute defined.",
            "/Configuration/Mapping/A");
    }

    [Fact]
    public void List_Override_AddUnparseable()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <List>
        <Item>3.1</Item>
    </List>
</Configuration>
";
        MergeInvalidSchema(
            overrideSchema,
            "Failed to parse '3.1' as a type of 'int'.",
            "/Configuration/List/Item");
    }


    [Fact]
    public void List_Override_ChangeTag()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <List>
        <NewItem>3.1</NewItem>
    </List>
</Configuration>
";
        MergeInvalidSchema(
            overrideSchema,
            "Expected tag name: 'Item'. Got: 'NewItem'.",
            "/Configuration/List/NewItem");
    }

    [Fact]
    public void Add_Key_To_Map_NotAllowed()
    {
        string overrideSchema =
@"
<Configuration xmlns:cdn=""http://configurator.net"">
    <SomethingElse>Foo</SomethingElse>
</Configuration>
";
        MergeInvalidSchema(
            overrideSchema,
            "Merged file contains key not present in base file. Merging may not add new keys.",
            "/Configuration/SomethingElse");
    }

    private static void MergeInvalidSchema(
        string overrideXml,
        string expectedError,
        string expectedPath)
    {
        TestErrorCollector tec = new();
        Assert.True(SchemaParser.TryParse(XDocument.Parse(BaseXml), tec, out var result));

        XElement @override = XDocument.Parse(overrideXml).Root!;
        Assert.False(result.MergeWith(@override, tec));

        Assert.Contains(tec.Errors, x => x.path == expectedPath && x.msg == expectedError);
    }

    private static void Merge(string overrideXml, string expectedXml)
    {
        var tec = new TestErrorCollector();
        Assert.True(SchemaParser.TryParse(XDocument.Parse(BaseXml), tec, out var result));

        XElement @override = XDocument.Parse(overrideXml).Root!;
        result!.MergeWith(@override, tec);

        string merged = result.XmlElement.ToString();

        Assert.Equal(expectedXml, merged);
        Assert.False(tec.HasErrors);
    }

    private class TestErrorCollector : IErrorCollector
    {
        public List<(string msg, string? path)> Errors = new();
        public List<(string msg, string? path)> Warnings = new();
        public List<(string msg, string? path)> Infos = new();

        public bool HasErrors => this.Errors.Count > 0;

        public void Error(string message, string? path = null)
        {
            this.Errors.Add((message, path));
        }

        public void Info(string message, string? path = null)
        {
            this.Infos.Add((message, path));
        }

        public void Warning(string message, string? path = null)
        {
            this.Warnings.Add((message, path));
        }
    }
}