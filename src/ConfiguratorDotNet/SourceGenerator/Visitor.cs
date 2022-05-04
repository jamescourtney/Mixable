using ConfiguratorDotNet.Schema;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

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

        foreach (var kvp in map.Children)
        {
            XName name = kvp.Key;
            SchemaElement value = kvp.Value;
            TypeContext valueType = value.Accept(this);

            properties.AddRange(valueType.Attributes);
            properties.Add($"public {valueType.TypeName} {name} {{ get; set; }}");
        }


        this.StringBuilder.AppendLine($"public class {className}");
        this.StringBuilder.AppendLine("{");
        
        foreach (string property in properties)
        {
            this.StringBuilder.AppendLine(property);
        }

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
        return element.GetDocumentPath().Trim('/').Replace('/', '_');
    }
}