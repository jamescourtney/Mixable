using Mixable.Core;
using Mixable.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Mixable.Python;

[ExcludeFromCodeCoverage]
public class TypeContext
{
    private readonly Func<string, string> createNew;

    public TypeContext(Func<string, string> createNew)
    {
        this.createNew = createNew;
    }

    public string CreateNew(string nodeVariable)
    {
        return createNew(nodeVariable);
    }
}

[ExcludeFromCodeCoverage]
public class SchemaVisitor : ISchemaVisitor<TypeContext>
{
    public IndentedCodeWriter CodeWriter { get; } = new(string.Empty, string.Empty, 2);

    public bool ShouldProcess(DocumentMetadata metadata)
    {
        return metadata.Python?.Enabled == true;
    }

    public void Run(SchemaElement element, DocumentMetadata metadata, IErrorCollector errorCollector)
    {
        if (!this.ShouldProcess(metadata))
        {
            return;
        }

        this.CodeWriter.AppendLine("from defusedxml.ElementTree import parse");
        this.CodeWriter.AppendLine(string.Empty);

        element.Accept(this, errorCollector);

        if (!string.IsNullOrEmpty(metadata.Python?.OutputFile))
        {
            System.IO.File.WriteAllText(metadata.Python.OutputFile, this.CodeWriter.StringBuilder.ToString());
        }
    }

    public TypeContext Accept(ListSchemaElement list, IErrorCollector errorCollector)
    {
        TypeContext innerType = list.Template.Accept(this, errorCollector);

        string variable1 = $"x{Guid.NewGuid():n}";
        string variable2= $"x{Guid.NewGuid():n}";

        string create = innerType.CreateNew(variable1);
        string childTagName = list.Template.XmlElement.Name.LocalName;

        return new TypeContext(parent => 
            $"[ {create} for {variable1} in filter(lambda {variable2}: {variable2}.tag == '{childTagName}', list({parent})) ]");
    }

    public TypeContext Accept(MapSchemaElement map, IErrorCollector errorCollector)
    {
        string className = GetClassName(map.XmlElement);

        List<string> initializations = new();

        foreach (var kvp in map.Children)
        {
            XName name = kvp.Key;
            SchemaElement value = kvp.Value;
            TypeContext valueType = value.Accept(this, errorCollector);

            string create = valueType.CreateNew($"element.find('{name.LocalName}')");
            initializations.Add($"self.{name.LocalName} = {create}");
        }

        this.CodeWriter.AppendLine($"class {className}:");
        using (this.CodeWriter.WithBlock())
        {
            this.CodeWriter.AppendLine("def __init__(self, element):");
            using (this.CodeWriter.WithBlock())
            {
                foreach (var init in initializations)
                {
                    this.CodeWriter.AppendLine(init);
                }
            }
        }

        return new TypeContext(x => $"{className}({x})");
    }

    public TypeContext Accept(ScalarSchemaElement scalar, IErrorCollector errorCollector)
    {
        Func<string, string, string> formatOptional = (v, t) => $"({t}.Parse({v}.text) if {v} != None ?  : default({t}))";
        Func<string, string, string> formatRequired = (v, t) => $"{t}.Parse({v}.Value)";

        switch ((scalar.ScalarType.Type, scalar.Modifier))
        {
            case (WellKnownType.Int, NodeModifier.Optional):
                return new TypeContext(s => $"(int({s}.text) if {s} != None else None)");

            case (WellKnownType.Int, _):
                return new TypeContext(s => $"int({s}.text)");

            case (WellKnownType.Double, NodeModifier.Optional):
                return new TypeContext(s => $"(float({s}.text) if {s} != None else None)");

            case (WellKnownType.Double, _):
                return new TypeContext(s => $"float({s}.text)");

            case (WellKnownType.Bool, NodeModifier.Optional):
                return new TypeContext(s => $"({s}.text.strip().lower() == \"true\" if {s} != None else None)");

            case (WellKnownType.Bool, _):
                return new TypeContext(s => $"{s}.text.strip().lower() == \"true\"");

            case (WellKnownType.String, NodeModifier.Optional):
                return new TypeContext(s => $"({s}.text if {s} != None else None)");

            case (WellKnownType.String, _):
                return new TypeContext(s => $"{s}.text");

            default:
                MixableInternal.Assert(false, "Unexpected type: " + scalar.ScalarType.Type);
                return null!; // won't reach here.
        }
    }

    private static string GetClassName(XElement element)
    {
        return element
            .GetDocumentPath(select: x => x.LocalName, where: x => x.Namespace != Constants.XMLNamespace)
            .Trim('/')
            .Replace('/', '_');
    }
}