using System.IO;
using Microsoft.CodeAnalysis;
using Mixable.Schema;

namespace Mixable.SourceGenerator;

[Generator]
public class MixableCSharpGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var item in context.AdditionalFiles)
        {
            if (Path.GetExtension(item.Path).ToLowerInvariant() == ".mxml")
            {
                MxmlFileProcessor processor = new MxmlFileProcessor(item.Path, new SourceGeneratorErrorCollector(ref context, item.Path));

                var visitor = new CSharp.SchemaVisitor(enableFileOutput: false);

                processor.MergeXml();

                if (processor.TryApplyVisitors(new ISchemaVisitor[] { visitor }))
                {
                    context.AddSource(Path.GetFileName(item.Path), visitor.CodeWriter.StringBuilder.ToString());
                }
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    { 
        // Nothing for us to do here.
    }
}
