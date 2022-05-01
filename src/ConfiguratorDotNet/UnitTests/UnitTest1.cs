using Xunit;

namespace UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string yaml =
@"
configuration:
  value1: !!str 3
  value2: 3.14
  value3: 4UL
  value4: 5L
  value6: true
  subItem:
    Foo: Bar
    Bar: Baz
  list:
  - 1
  - 2
  - 3
  classList:
  - a: foo
    b: bar
  - a: foo2
    b: bar2
";
            var item = ConfiguratorDotNet.Generator.YamlSchemaReader.ParseSchema(yaml);
        }
    }
}