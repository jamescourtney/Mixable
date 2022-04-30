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
  value1: 3
  value2: 3.14
  value3: 4UL
  value4: 5L
  value6: true
  value1: repeated
  subItem:
    Foo: Bar
    Bar: Baz
";
            var item = ConfiguratorDotNet.Generator.YamlSchemaReader.ParseSchema(yaml);
        }
    }
}