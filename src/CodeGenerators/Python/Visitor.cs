using Mixable.Schema;
using System;
using System.Text;
using System.Xml.Linq;

namespace Mixable.Python;

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

public class PythonCodeWriter
{
    private const string OneIdent = "  ";

    private int indentSize;

    public StringBuilder StringBuilder { get; } = new();

    public IDisposable WithBlock()
    {
        return new SimpleDisposable(this);
    }

    public void AppendLine(string line)
    {
        for (int i = 0; i < this.indentSize; ++i)
        {
            this.StringBuilder.Append(OneIdent);
        }

        this.StringBuilder.AppendLine(line);
    }

    private class SimpleDisposable : IDisposable
    {
        private readonly PythonCodeWriter writer;

        public SimpleDisposable(PythonCodeWriter writer)
        {
            this.writer = writer;
            this.writer.indentSize++;
        }

        public void Dispose()
        {
            writer.indentSize--;
        }
    }
}

public class SchemaVisitor : ISchemaVisitor<TypeContext>
{
    public PythonCodeWriter CodeWriter { get; } = new();

    public void Finish()
    {
    }

    public void Initialize(DocumentMetadata metadata)
    {
        this.CodeWriter.AppendLine("from defusedxml.ElementTree import parse");
        this.CodeWriter.AppendLine(string.Empty);
    }

    public TypeContext Accept(ListSchemaElement list)
    {
        TypeContext innerType = list.Template.Accept(this);

        string variable1 = Guid.NewGuid().ToString("n");
        string variable2= Guid.NewGuid().ToString("n");

        string create = innerType.CreateNew(variable1);
        string childTagName = list.Template.XmlElement.Name.LocalName;

        return new TypeContext(parent => 
            $"[ {create} for {variable1} in filter(lambda {variable2}: {variable2}.tag == '{childTagName}', {parent}.getchildren()) ]");
    }

    public TypeContext Accept(MapSchemaElement map)
    {
        string className = GetClassName(map.XmlElement);

        this.CodeWriter.AppendLine($"class {className}:");
        using (this.CodeWriter.WithBlock())
        {
            this.CodeWriter.AppendLine("def __init__(self, element):");
            using (this.CodeWriter.WithBlock())
            {
                foreach (var kvp in map.Children)
                {
                    XName name = kvp.Key;
                    SchemaElement value = kvp.Value;
                    TypeContext valueType = value.Accept(this);

                    string create = valueType.CreateNew($"element.find('{name.LocalName}')");
                    this.CodeWriter.AppendLine($"self.{name.LocalName} = {create}");
                }
            }
        }

        return new TypeContext(x => $"{className}(x)");
    }

    public TypeContext Accept(ScalarSchemaElement scalar)
    {
        switch (scalar.ScalarType.Type)
        {
            case WellKnownType.Int:
                return new TypeContext(s => $"int({s}.text)");

            case WellKnownType.Double:
                return new TypeContext(s => $"float({s}.text)");

            case WellKnownType.Bool:
                return new TypeContext(s => $"{s}.text.strip().lower() == \"true\"");

            case WellKnownType.String:
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