namespace UnitTests;

public class MergeTests
{
    private const string BaseXml =
@"
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata>
        <mx:NamespaceName>Foo.Bar.Baz</mx:NamespaceName>
    </mx:Metadata>

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
        <mx:ListItemTemplate>
            <Item>
                <Value mx:Type=""double"">0</Value>
                <SomethingElse mx:Optional=""true"">0</SomethingElse>
            </Item>
        </mx:ListItemTemplate>
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
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <mx:Metadata>
        <mx:BaseFile>base.xml</mx:BaseFile>
    </mx:Metadata>

    <List>
       <Item>4</Item>
    </List>

    <Mapping>
        <A>5</A>
    </Mapping>

    <FancyList mx:ListMerge=""Replace"">
        <Item>
            <Value>2.3</Value>
            <SomethingElse>10</SomethingElse>
        </Item>
        <Item><Value>1.2</Value></Item>
    </FancyList>
</Configuration>
";
        string expected =
@"<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
  <mx:Metadata>
    <mx:NamespaceName>Foo.Bar.Baz</mx:NamespaceName>
  </mx:Metadata>
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
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
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
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
    <Mapping>
        <A mx:Type=""bool"">true</A>
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
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
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
<Configuration xmlns:mx=""https://github.com/jamescourtney/mixable"">
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
}