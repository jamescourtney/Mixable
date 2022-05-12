using Mixable.Schema;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Mixable.CSharp;

public class TypeContext
{
    public List<string> Attributes { get; } = new();

    public string? TypeName { get; init; }
}

public class SchemaVisitor : ISchemaVisitor<TypeContext>
{
    private readonly bool enableFileOutput;

    public SchemaVisitor(bool enableFileOutput)
    {
        this.enableFileOutput = enableFileOutput;
    }

    public StringBuilder StringBuilder { get; } = new();

    public bool ShouldProcess(DocumentMetadata metadata)
    {
        return metadata.CSharp?.Enabled == true;
    }

    public void Run(SchemaElement element, DocumentMetadata metadata, IErrorCollector errorCollector)
    {
        if (!this.ShouldProcess(metadata) || metadata.CSharp is null)
        {
            return;
        }

        this.StringBuilder.AppendLine($"namespace {metadata.CSharp.NamespaceName}");
        this.StringBuilder.AppendLine("{");
        this.StringBuilder.AppendLine("using System.Collections.Generic;");
        this.StringBuilder.AppendLine("using System.Xml.Serialization;");

        element.Accept(this, errorCollector);

        this.StringBuilder.AppendLine("}");

        if (!string.IsNullOrEmpty(metadata.CSharp.OutputFilePath) && this.enableFileOutput)
        {
            System.IO.File.WriteAllText(metadata.CSharp.OutputFilePath, this.StringBuilder.ToString());
        }
    }

    public TypeContext Accept(ListSchemaElement list, IErrorCollector errorCollector)
    {
        TypeContext innerType = list.Template.Accept(this, errorCollector);

        var context = new TypeContext
        {
            TypeName = $"List<{innerType.TypeName}>",
        };
        
        context.Attributes.Add($"[XmlArray(ElementName = \"{list.XmlElement.Name}\")]");
        context.Attributes.Add($"[XmlArrayItem(ElementName = \"{list.Template.XmlElement.Name}\")]");

        return context;
    }

    public TypeContext Accept(MapSchemaElement map, IErrorCollector errorCollector)
    {
        string className = GetClassName(map.XmlElement);

        List<string> properties = new();
        List<string> caseStatements = new();

        foreach (var kvp in map.Children)
        {
            XName name = kvp.Key;
            SchemaElement value = kvp.Value;
            TypeContext valueType = value.Accept(this, errorCollector);

            properties.AddRange(valueType.Attributes);
            properties.Add($"public {valueType.TypeName} {name} {{ get; set; }}");

            caseStatements.Add($"case \"{name}\": {{ child = this.{name}; return true; }}");
        }

        this.StringBuilder.AppendLine($"public partial class {className}");
        this.StringBuilder.AppendLine("{");

        foreach (string property in properties)
        {
            this.StringBuilder.AppendLine(property);
        }

        this.StringBuilder.AppendLine(@$"
    public bool TryGetChild(string name, out object child)
    {{
            child = null;
            switch (name)
            {{
                {string.Join("\r\n", caseStatements)}
            }}

            return false;
    }}");

        this.StringBuilder.AppendLine($@"
    public object this[string name]
    {{
        get
        {{
            object child;
            if (this.TryGetChild(name, out child))
            {{
                return child;
            }}

            throw new KeyNotFoundException();
        }}
    }}

    ");

        this.StringBuilder.AppendLine("}");

        return new()
        {
            TypeName = className,
        };
    }

    public TypeContext Accept(ScalarSchemaElement scalar, IErrorCollector errorCollector)
    {
        return new TypeContext
        {
            TypeName = scalar.ScalarType.Type.ToString().ToLowerInvariant(),
        };
    }

    private static string GetClassName(XElement element)
    {
        return element
            .GetDocumentPath(select: x => x.LocalName, where: x => x.Namespace != Constants.XMLNamespace)
            .Trim('/')
            .Replace('/', '_');
    }
}