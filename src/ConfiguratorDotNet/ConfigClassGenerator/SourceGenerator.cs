using System.IO;
using Microsoft.CodeAnalysis;

namespace ConfigClassGenerator;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var file in context.AdditionalFiles)
        {
            if (file.Path.EndsWith(".cgyml", StringComparison.InvariantCultureIgnoreCase))
            {
                YamlStream stream = new YamlStream();
                stream.Load(File.OpenText(file.Path));
                YamlDocument document = stream.Documents[0];
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }
}