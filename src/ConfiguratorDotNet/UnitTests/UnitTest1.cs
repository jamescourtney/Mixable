using ConfiguratorDotNet.Generator;
using System.Xml.Linq;
using Xunit;

namespace UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string xml =
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

            var result = SchemaParser.Parse(XDocument.Parse(xml));

            result.Validate();

            result.MergeWith(XDocument.Parse(overrideSchema).Root, new DerivedSchemaAttributeValidator());

        }
    }
}