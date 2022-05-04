using ConfiguratorDotNet.Schema;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Xml.Linq;

namespace SourceGenerator;

public static class Constants
{
    public static readonly DiagnosticDescriptor CdnXmlNometadata = new("CDN0001", "", "", "ConfiguratorDotNet", DiagnosticSeverity.Warning, true);
}

[Generator]
public class Class1 : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        Console.WriteLine("Hi!");
        try
        {
            foreach (var additionalFile in context.AdditionalFiles)
            {
                if (additionalFile.Path.ToLowerInvariant().EndsWith(".cdn.xml"))
                {
                    string text = additionalFile.GetText()?.ToString();
                    if (!DocumentMetadata.TryCreateFromXml(text, out var metadata))
                    {
                        // report error
                        Console.WriteLine("Can't create metadata");
                        continue;
                    }

                    if (!metadata.ValidateAsTemplateFile(out string templateError) &&
                        !metadata.ValidateAsOverrideFile(out string overrideError))
                    {
                        // template file! -- ignore
                        continue;
                    }

                    SchemaElement element = this.ProcessFile(additionalFile.Path);

                    if (!string.IsNullOrEmpty(metadata.OutputXmlFileName))
                    {
                        string relativePath = Path.Combine(
                            Path.GetDirectoryName(additionalFile.Path),
                            metadata.OutputXmlFileName);

                        element.XmlElement.Save(relativePath);
                    }

                    if (metadata.GenerateCSharp == true)
                    {
                        var visitor = new SchemaVisitor("Foo.Bar.Baz");

                        element.Accept(visitor);
                        visitor.Finish();

                        context.AddSource(
                            Path.GetFileNameWithoutExtension(additionalFile.Path),
                            visitor.StringBuilder.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    private SchemaElement ProcessFile(string path)
    {
        SchemaElement? baseSchema = null;

        string text = File.ReadAllText(path);
        if (DocumentMetadata.TryCreateFromXml(text, out DocumentMetadata? metadata))
        {
            if (!string.IsNullOrEmpty(metadata.BaseFileName))
            {
                string baseFilePath = Path.IsPathRooted(metadata.BaseFileName)
                    ? metadata.BaseFileName
                    : Path.Combine(Path.GetDirectoryName(path), metadata.BaseFileName);

                baseSchema = this.ProcessFile(baseFilePath);
            }
        }

        XDocument document = XDocument.Parse(text);
        if (baseSchema is not null)
        {
            if (baseSchema.MatchesSchema(document.Root, new DerivedSchemaAttributeValidator(), out string mismatchPath, out string error))
            {
                baseSchema.MergeWith(document.Root, new DerivedSchemaAttributeValidator());
            }
            else
            {
                throw new Exception(error);
            }
        }
        else
        {
            baseSchema = SchemaParser.Parse(document);
        }

        return baseSchema;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }
}
