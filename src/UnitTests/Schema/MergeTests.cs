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
        <cdn:ListItemTemplate>
            <Item>
                <Value cdn:Type=""double"">0</Value>
                <SomethingElse cdn:Optional=""true"">0</SomethingElse>
            </Item>
        </cdn:ListItemTemplate>
        <Item>
            <Value>1</Value>
        </Item>
        <Item>
            <Value>2</Value>
            <SomethingElse>11</SomethingElse>
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

    <FancyList cdn:ListMerge=""Replace"">
        <Item>
            <Value>2.3</Value>
            <SomethingElse>10</SomethingElse>
        </Item>
        <Item><Value>1.2</Value></Item>
    </FancyList>
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
  <FancyList>
    <Item>
      <Value>2.3</Value>
      <SomethingElse>10</SomethingElse>
    </Item>
    <Item>
      <Value>1.2</Value>
    </Item>
  </FancyList>
</Configuration>";

        MergeHelpers.Merge(BaseXml, overrideSchema, expected);
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
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
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
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
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
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
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
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
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
        MergeHelpers.MergeInvalidSchema(
            BaseXml,
            overrideSchema,
            "Merged file contains key not present in base file. Merging may not add new keys.",
            "/Configuration/SomethingElse");
    }
}