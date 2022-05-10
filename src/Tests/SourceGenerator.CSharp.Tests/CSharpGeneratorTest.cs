using Microsoft.CodeAnalysis;
using System.IO;

namespace CSharp.SourceGenerator.Tests;

public class CSharpGeneratorTest
{
    [Fact]
    public void GeneralTest()
    {
        RunAndCompileAsync(
            new[] { "XML/GeneralTest/Base.mxml", "XML/GeneralTest/Derived.mxml", "XML/GeneralTest/Derived2.mxml" },
            out var diagnostics,
            out var compilation,
            out var compilationResult,
            out var assembly);

        Assert.Empty(diagnostics);
        Assert.True(compilationResult.Success);
        Assert.NotNull(assembly);

        System.Xml.Serialization.XmlSerializer s = new(assembly.GetType("Foo.Bar.Baz.Bat.Configuration"));
        dynamic value = s.Deserialize(
            new StringReader(
                File.ReadAllText(MakeAbsolutePath("XML/GeneralTest/Derived2.xml"))));

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

    [Fact]
    public void CycleTest()
    {
        RunAndCompileAsync(
            new[] { "XML/CycleTest/Base.mxml" },
            out var diagnostics,
            out var compilation,
            out var compilationResult,
            out var assembly);

        Assert.NotEmpty(diagnostics);
        Assert.Null(assembly);
        Assert.Null(compilationResult);

        Assert.Contains(
            diagnostics,
            d => d.Severity == DiagnosticSeverity.Error
              && d.GetMessage().Contains("Cycle detected in include files."));
    }
}