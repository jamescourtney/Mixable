using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

using Mixable.SourceGenerator;

namespace CSharp.SourceGenerator.Tests;

public static class TestHarness
{
    public static bool RunAndCompileAsync(
        string[] sourceFiles,
        out ImmutableArray<Diagnostic> diagnostics,
        out Compilation outputCompilation,
        out EmitResult? compilationResult,
        out Assembly? assembly)
    {
        var driver = CSharpGeneratorDriver.Create(
            new[] { new MixableCSharpGenerator() },
            sourceFiles.Select(x => (CustomAdditionalText)x).ToArray());

        Compilation input = CreateCompilation();

        compilationResult = null;
        assembly = null;

        driver.RunGeneratorsAndUpdateCompilation(
            input,
            out outputCompilation,
            out diagnostics);

        if (diagnostics.Any(x => x.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error))
        {
            return false;
        }
        
        // Our generated file compiles!
        string tempfilePath = $"{Path.GetTempFileName()}.dll";
        compilationResult = outputCompilation.Emit(tempfilePath);

        // if we got to the point of trying to compile, we should expect success.
        Assert.True(compilationResult.Success);

        assembly = Assembly.LoadFile(tempfilePath);
        return true;
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

    public static string MakeAbsolutePath(string relativePath)
    {
        string directory = Path.GetDirectoryName(typeof(CSharpGeneratorTest).Assembly.Location);
        return Path.Combine(directory, relativePath);
    }
}