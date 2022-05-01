using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ConfiguratorDotNet.Runtime;

namespace ConfiguratorDotNet.Generator;

internal abstract class SchemaElement
{
    /// <summary>
    /// Gets or sets the type name for the current schema element.
    /// </summary>
    public string? TypeName { get; set;  }

    public SchemaElement? Parent { get; set; }

    public abstract string ToCSharp();
}

internal enum ListMergePolicy
{
    Concatenate = 0,
    Replace = 1,
}

internal class YamlSchemaReader
{
    public static SchemaElement? ParseSchema(string yaml)
    {
        YamlStream stream = new();
        stream.Load(new StringReader(yaml));

        YamlVisitor visitor = new();

        stream.Documents[0].Accept(visitor);

        if (visitor.Value is not null)
        {

        }

        string csharp = string.Join("\r\n\r\n", visitor.AllMaps.Select(x => x.ToCSharp()));

        return visitor.Value;
    }
}

internal static class WellKnownTags
{
    private static readonly Dictionary<string, string> ScalarTags = new()
    {
        { "tag:yaml.org,2002:null", "string" },
        { "tag:yaml.org,2002:bool", "bool" },
        { "tag:yaml.org,2002:str", "string" },
        { "tag:yaml.org,2002:int", "int" },
        { "tag:yaml.org,2002:float", "double" },
        { "tag:yaml.org,2002:cdn/float32", "float" },
        { "tag:yaml.org,2002:cdn/int64", "long" },
    };

    private static readonly Dictionary<string, string> SequenceTags = new()
    {
        { "tag:yaml.org,2002:omap", "string" },
        { "tag:yaml.org,2002:pairs", "bool" },
        { "tag:yaml.org,2002:str", "string" },
        { "tag:yaml.org,2002:int", "int" },
        { "tag:yaml.org,2002:float", "double" },
        { "tag:yaml.org,2002:cdn/float32", "float" },
        { "tag:yaml.org,2002:cdn/int64", "long" },
    };

    public static bool TryResolveScalarType(
        YamlScalarNode scalarNode,
        [NotNullWhen(true)] out string? type)
    {
        if (string.IsNullOrEmpty(scalarNode.Tag.Value))
        {
            type = "string";
            return true;
        }

        return ScalarTags.TryGetValue(scalarNode.Tag.Value, out type);
    }

    public static bool TryResolveSequenceType(
        YamlSequenceNode sequenceNode,
        string childNodeType,
        [NotNullWhen(true)] out string? type)
    {
        string? tagValue = sequenceNode.Tag.Value;
        if (tagValue == "tag:yaml.org,2002:seq")
        {
            type = $"List<{childNodeType}>";
            return true;
        }

        if (tagValue == "tag:yaml.org,2002:set")
        {
            type = $"HashSet<{childNodeType}>";
            return true;
        }

        type = $"List<{childNodeType}>";
        return true;
    }


    public static bool TryResolveMappingType(
        YamlMappingNode mappingNode,
        string keyNodeType,
        string valueNodeType,
        [NotNullWhen(true)] out string? type)
    {
        string? tagValue = mappingNode.Tag.Value;
        if (tagValue == "tag:yaml.org,2002:pairs" ||
            tagValue == "tag:yaml.org,2002:omap")
        {
            type = $"List<KeyValuePair<{keyNodeType}, {valueNodeType}>>";
            return true;
        }

        if (tagValue == "tag:yaml.org,2002:map")
        {
            type = $"Dictionary<{keyNodeType}, {valueNodeType}>";
            return true;
        }

        type = null;
        return false;
    }
}

internal class ScalarSchemaElement : SchemaElement
{
    public ScalarSchemaElement(YamlScalarNode value)
    {
        if (value.Tag == new YamlDotNet.Core.TagName("tag:yaml.org,2002:str"))
        {
            this.TypeName = "string";
        }
        else if (!string.IsNullOrEmpty(value.Value))
        {
            this.TypeName = ClassifyValue(value.Value);
        }
        else
        {
            throw new ConfiguratorDotNetException("Null value detected.");
        }
    }

    public override string ToCSharp()
    {
        throw new NotImplementedException();
    }
}

internal class ListSchemaElement : SchemaElement
{
    private readonly IReadOnlyList<SchemaElement> children;

    public ListSchemaElement(IReadOnlyList<SchemaElement> children)
    {
        foreach (var child in children)
        {
            child.Parent = this;
        }

        this.children = children;
    }

    public override string ToCSharp()
    {
        throw new NotImplementedException();
    }
}

internal class MapSchemaElement : SchemaElement
{
    private readonly IReadOnlyDictionary<string, SchemaElement> children;

    public MapSchemaElement(IReadOnlyDictionary<string, SchemaElement> children)
    {
        this.children = children;
        foreach (var kvp in this.children)
        {
            kvp.Value.Parent = this;
        }
    }

    public override string ToCSharp()
    {
        StringBuilder sb = new();

        foreach (var kvp in this.children)
        {
            sb.AppendLine($"public {kvp.Value.TypeName} {kvp.Key} {{ get; set; }}");
        }

        return
$@"
        public class {this.TypeName}
        {{
            {sb}
        }}
";
    }
}

internal class YamlVisitor : IYamlVisitor
{
    public YamlVisitor()
    {
        this.NameStack = new();
        this.AllMaps = new();
    }

    public YamlVisitor(YamlVisitor source)
    {
        this.NameStack = source.NameStack;
        this.AllMaps = source.AllMaps;
    }

    public SchemaElement? Value { get; set; }

    public List<MapSchemaElement> AllMaps { get; }

    public TypeNameStack NameStack { get; }
    
    public void Visit(YamlStream stream)
    {
        throw new NotImplementedException();
    }

    public void Visit(YamlDocument document)
    {
        this.NameStack.Push("Configuration");
        document.RootNode.Accept(this);
    }

    public void Visit(YamlScalarNode scalar)
    {
        this.Value = new ScalarSchemaElement(scalar);
    }

    public void Visit(YamlSequenceNode sequence)
    {
        YamlVisitor visitor = new(this);
        List<SchemaElement> elements = new();

        this.NameStack.Push("Item");
        try
        {

            foreach (var item in sequence.Children)
            {
                visitor.Value = null;

                item.Accept(visitor);

                if (visitor.Value is not null)
                {
                    elements.Add(visitor.Value);
                }
            }
        }
        finally
        {
            this.NameStack.Pop();
        }

        this.Value = new ListSchemaElement(elements) { TypeName = $"List<{elements[0].TypeName}>" };
    }

    public void Visit(YamlMappingNode mapping)
    {
        YamlVisitor visitor = new(this);
        Dictionary<string, SchemaElement> elements = new();

        foreach (var kvp in mapping.Children)
        {
            visitor.Value = null;
            
            if (kvp.Key is not YamlScalarNode scalarKey || string.IsNullOrEmpty(scalarKey.Value))
            {
                throw new ConfiguratorDotNetException("Mapipngs must have non-null string keys.");
            }

            this.NameStack.Push(scalarKey.Value!);
            try
            {
                kvp.Value.Accept(visitor);

                if (visitor.Value is null)
                {
                    throw new ConfiguratorDotNetException("Mapping values may not be null.");
                }

                elements.Add(scalarKey.Value!, visitor.Value!);
            }
            finally
            {
                this.NameStack.Pop();
            }
        }

        var se = new MapSchemaElement(elements)
        {
            TypeName = string.Join("_", this.NameStack)
        };

        this.Value = se;

        this.AllMaps.Add(se);
    }
}

internal class TypeNameStack
{
    private readonly LinkedList<string> parts = new();

    public void Push(string value)
    {
        this.parts.AddLast(value);
    }

    public void Pop()
    {
        this.parts.RemoveLast();
    }

    public override string ToString()
    {
        return string.Join("_", this.parts);
    }
}