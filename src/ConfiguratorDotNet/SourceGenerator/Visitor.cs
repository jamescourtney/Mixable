using ConfiguratorDotNet.Schema;
using System.Text;

namespace SourceGenerator;

public class TypeContext
{
    public List<string> Attributes { get; } = new();

    public string TypeName { get; init; }
}

public class SchemaVisitor : ISchemaVisitor<TypeContext>
{
    public StringBuilder StringBuilder { get; } = new();

    public SchemaVisitor(string @namespace)
    {
        this.StringBuilder.AppendLine($"namespace {@namespace}");
        this.StringBuilder.AppendLine("{");
        this.StringBuilder.AppendLine("using System.Collections.Generic;");
        this.StringBuilder.AppendLine("using System.Xml.Serialization;");
    }

    public void Finish()
    {
        this.StringBuilder.Append("}");
    }

    public TypeContext Accept(ListSchemaElement list)
    {
        TypeContext innerType = list.Template.Accept(this);

        var context = new TypeContext
        {
            TypeName = $"List<{innerType.TypeName}>",
        };
        
        context.Attributes.Add($"[XmlArray(ElementName = \"{list.XmlElement.Name}\")]");
        context.Attributes.Add($"[XmlArrayItem(ElementName = \"{list.Template.XmlElement.Name}\")]");

        return context;
    }

    public TypeContext Accept(MapSchemaElement map)
    {
        string className = GetClassName(map.XmlElement);

        List<string> properties = new();
        List<string> caseStatements = new();

        foreach (var kvp in map.Children)
        {
            XName name = kvp.Key;
            SchemaElement value = kvp.Value;
            TypeContext valueType = value.Accept(this);

            properties.AddRange(valueType.Attributes);
            properties.Add($"public {valueType.TypeName} {name} {{ get; set; }}");

            caseStatements.Add($"case \"{name}\": {{ child = this.{name}; return true; }}");
        }

        this.StringBuilder.AppendLine($"public class {className}");
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

    public TypeContext Accept(ScalarSchemaElement scalar)
    {
        return new TypeContext
        {
            TypeName = scalar.ScalarType.TypeName
        };
    }

    private static string GetClassName(XElement element)
    {
        return element.GetDocumentPath(x => x.LocalName).Trim('/').Replace('/', '_');
    }
}