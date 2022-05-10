using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Mixable.SourceGenerator;
using System.IO;
using System.Reflection;
using System.Threading;

namespace UnitTests;

public class CSharpGeneratorTest
{
    public static string MakeAbsolutePath(string relativePath)
    {
        string directory = Path.GetDirectoryName(typeof(CSharpGeneratorTest).Assembly.Location);
        return Path.Combine(directory, relativePath);
    }

    [Fact]
    public void TestAsync()
    {
        var driver = CSharpGeneratorDriver.Create(
            new[] { new MixableCSharpGenerator() },
            new CustomAdditionalText[] { "XML/Base.mxml", "XML/Derived.mxml", "XML/Derived2.mxml" });

        Compilation input = CreateCompilation();

        driver.RunGeneratorsAndUpdateCompilation(input, out var outputCompilation, out var diagnostics);
        Assert.True(diagnostics.IsEmpty);
        
        // Our generated file compiles!
        string tempfilePath = $"{Path.GetTempFileName()}.dll";
        var result = outputCompilation.Emit(tempfilePath);
        Assert.True(result.Success);

        var asm = Assembly.LoadFile(tempfilePath);

        System.Xml.Serialization.XmlSerializer s = new(asm.GetType("Foo.Bar.Baz.Bat.Configuration"));
        dynamic value = s.Deserialize(new StringReader(File.ReadAllText(MakeAbsolutePath("XML/Derived2.xml"))));

        Assert.Equal(4, (int)value.Mapping.A);
        Assert.Equal("derived2.xml", (string)value.Mapping.B);
        Assert.Equal(3.14159, (double)value.Mapping.C.C);

        Assert.Empty(value.List);

        Assert.Equal(2, ((IEnumerable<object>)value.FancyList).Count());
        Assert.Equal(1000, (int)value.FancyList[0].Value);
        Assert.Equal(0, (int)value.FancyList[0].OtherValue);
        Assert.Equal(1001, (int)value.FancyList[1].Value);
        Assert.Equal(1002, (int)value.FancyList[1].OtherValue); // not present.

        Assert.Equal(2, ((IEnumerable<int>)value.AbstractList).Count());
        Assert.Equal(1, (int)value.AbstractList[0]);
        Assert.Equal(2, (int)value.AbstractList[1]);
    }

    private class CustomAdditionalText : AdditionalText
    {
        public CustomAdditionalText(string relativePath)
        {
            this.Path = MakeAbsolutePath(relativePath);
        }

        public override string Path { get; }

        public override SourceText? GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(File.ReadAllText(this.Path));
        }

        public static implicit operator CustomAdditionalText(string path)
        {
            return new CustomAdditionalText(path);
        }
    }

    private static Compilation CreateCompilation()
    {
        List<MetadataReference> refs = new();

        refs.Add(MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(typeof(System.Xml.Serialization.XmlArrayAttribute).Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

        refs.AddRange(
            new[] { "mscorlib.dll", "System.dll", "System.Core.dll", "System.Runtime.dll" }
            .Select(x => Path.Combine(assemblyPath, x))
            .Select(x => MetadataReference.CreateFromFile(x)));

        return CSharpCompilation.Create(
            "compilation.dll",
            references: refs,
            options: 
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithPlatform(Platform.AnyCpu)
                .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default));
    }
}