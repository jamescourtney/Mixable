using System.IO;
using Microsoft.CodeAnalysis;
using Mixable.Schema;

namespace Mixable.SourceGenerator;

[Generator]
public class MixableCSharpGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        MxmlFileProcessor processor = new();

        foreach (var item in context.AdditionalFiles)
        {
            if (Path.GetExtension(item.Path).ToLowerInvariant() == ".mxml")
            {
                var visitor = new CSharp.SchemaVisitor();

                if (!processor.TryProcessFile(
                    item.Path,
                    visitor,
                    new SourceGeneratorErrorCollector(ref context, item.Path),
                    out var metadata))
                {
                    // Nothing was done.
                    continue;
                }

                visitor.Finish();

                if (metadata.GenerateCSharp == true)
                {
                    context.AddSource(Path.GetFileName(item.Path), visitor.StringBuilder.ToString());
                }
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    { 
        // Nothing for us to do here.
    }
}
