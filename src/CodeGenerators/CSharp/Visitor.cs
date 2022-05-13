using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Mixable.Core;
using Mixable.Schema;

namespace Mixable.CSharp;

public class TypeContext
{
    public TypeContext(string typeName, Func<string, string> getAssignStatement)
    {
        this.TypeName = typeName;
        this.GetAssignStatement = getAssignStatement;
    }

    public string TypeName { get; init; }

    public Func<string, string> GetAssignStatement { get; init; }
}

public class SchemaVisitor : ISchemaVisitor<TypeContext>
{
    private readonly bool enableFileOutput;

    public SchemaVisitor(bool enableFileOutput)
    {
        this.enableFileOutput = enableFileOutput;
    }

    public IndentedCodeWriter CodeWriter { get; } = new("{", "}", 4);

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

        this.CodeWriter.AppendLine($"namespace {metadata.CSharp.NamespaceName}");

        using (this.CodeWriter.WithBlock())
        {
            this.CodeWriter.AppendLine("using System.Collections.Generic;");
            this.CodeWriter.AppendLine("using System.Xml.XPath;");
            this.CodeWriter.AppendLine("using System.Xml.Linq;");
            this.CodeWriter.AppendLine("using System.Linq;");
            this.CodeWriter.AppendLine(string.Empty);

            element.Accept(this, errorCollector);
        }

        if (!string.IsNullOrEmpty(metadata.CSharp.OutputFilePath) && this.enableFileOutput)
        {
            System.IO.File.WriteAllText(metadata.CSharp.OutputFilePath, this.CodeWriter.StringBuilder.ToString());
        }
    }

    public TypeContext Accept(ListSchemaElement list, IErrorCollector errorCollector)
    {
        TypeContext innerType = list.Template.Accept(this, errorCollector);

        string randomVariableName = GetVariableName();
        var context = new TypeContext(
            $"List<{innerType.TypeName}>",
            x => $"{x}.XPathSelectElements(\"{list.Template.XmlElement.Name.LocalName}\").Select({randomVariableName} => {innerType.GetAssignStatement(randomVariableName)}).ToList()");

        return context;
    }

    public TypeContext Accept(MapSchemaElement map, IErrorCollector errorCollector)
    {
        string className = GetClassName(map.XmlElement);

        List<(string name, TypeContext ctx)> childContexts = new();

        foreach (var kvp in map.Children)
        {
            XName name = kvp.Key;
            SchemaElement value = kvp.Value;
            childContexts.Add((name.LocalName, value.Accept(this, errorCollector)));
        }

        this.CodeWriter.AppendLine($"public partial class {className}");

        using (this.CodeWriter.WithBlock())
        {
            this.CodeWriter.AppendLine($"public {className}(XElement element)");
            using (this.CodeWriter.WithBlock())
            {
                foreach (var (name, ctx) in childContexts)
                {
                    this.CodeWriter.AppendLine($"var temp{name} = element.XPathSelectElement(\"{name}\");");

                    string initStatement = ctx.GetAssignStatement($"temp{name}");
                    this.CodeWriter.AppendLine($"this.{name} = {initStatement};");
                }
            }

            foreach (var (name, ctx) in childContexts)
            {
                this.CodeWriter.AppendLine($"public {ctx.TypeName} {name} {{ get; }}");
            }
        }

        return new(className, x => $"new {className}({x})");
    }

    public TypeContext Accept(ScalarSchemaElement scalar, IErrorCollector errorCollector)
    {
        bool optional = scalar.Modifier == NodeModifier.Optional;

        Func<string, string, string> formatOptional = (v, t) => $"({v} is not null ? {t}.Parse({v}.Value) : default({t}))";
        Func<string, string, string> formatRequired = (v, t) => $"{t}.Parse({v}.Value)";

        switch (scalar.ScalarType.Type)
        {
            case WellKnownType.Bool:
                return new TypeContext(
                    "bool",
                    x => optional ? formatOptional(x, "bool") : formatRequired(x, "bool"));

            case WellKnownType.String:
                return new TypeContext(
                    "string",
                    x => optional ? $"{x}?.Value" : $"{x}.Value");

            case WellKnownType.Double:
                return new TypeContext(
                    "double",
                    x => optional ? formatOptional(x, "double") : formatRequired(x, "double"));

            case WellKnownType.Int:
                return new TypeContext(
                    "int",
                    x => optional ? formatOptional(x, "int") : formatRequired(x, "int"));
        }

        MixableInternal.Assert(false, "Unknown well known type: " + scalar.ScalarType.Type);
        return null!;
    }

    private static string GetClassName(XElement element)
    {
        return element
            .GetDocumentPath(select: x => x.LocalName, where: x => x.Namespace != Constants.XMLNamespace)
            .Trim('/')
            .Replace('/', '_');
    }

    private static string GetVariableName()
    {
        return $"v_{Guid.NewGuid():n}";
    }
}