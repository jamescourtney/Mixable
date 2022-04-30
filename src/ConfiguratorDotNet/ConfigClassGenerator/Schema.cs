using ConfiguratorDotNet.Runtime;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace ConfiguratorDotNet.Generator;

internal interface ISchemaElement : IEnumerable<ISchemaElement>
{
    /// <summary>
    /// Gets the type name for the current schema element.
    /// </summary>
    string TypeName { get; }

    ISchemaElement? Parent { get; set; }
}

internal enum ListMergePolicy
{
    Concatenate = 0,
    Replace = 1,
}

internal class YamlSchemaReader
{
    public static ISchemaElement? ParseSchema(string yaml)
    {
        YamlStream stream = new();
        stream.Load(new StringReader(yaml));

        YamlVisitor visitor = new();

        stream.Documents[0].Accept(visitor);

        return visitor.Value;
    }
}

internal class ScalarSchemaElement : ISchemaElement
{
    private static Regex BoolRegex = new Regex(@"^(true)|(false)|(True)|(False)$", RegexOptions.Compiled);
    private static Regex IntRegex = new Regex(@"^\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex UIntRegex = new Regex(@"^\d+u$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex ULongRegex = new Regex(@"^\d+ul$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex LongRegex = new Regex(@"^\d+l$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ScalarSchemaElement(string value)
    {
        this.TypeName = ClassifyValue(value);
    }

    public ISchemaElement? Parent { get; set; }

    public string TypeName { get; private init; }

    public IEnumerator<ISchemaElement> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield break;
    }

    private static string ClassifyValue(string value)
    {
        value = value.Trim();

        if (BoolRegex.IsMatch(value))
        {
            return "bool";
        }
        else if (IntRegex.IsMatch(value))
        {
            return "int";
        }
        else if (UIntRegex.IsMatch(value))
        {
            return "uint";
        }
        else if (ULongRegex.IsMatch(value))
        {
            return "ulong";
        }
        else if (LongRegex.IsMatch(value))
        {
            return "long";
        }
        else if (double.TryParse(value, out _))
        {
            return "double";
        }

        return "string";
    }
}

internal class ListSchemaElement : ISchemaElement
{
    private readonly IReadOnlyList<ISchemaElement> children;

    public ListSchemaElement(IReadOnlyList<ISchemaElement> children)
    {
        foreach (var child in children)
        {
            child.Parent = this;
        }

        this.children = children;
    }

    public ISchemaElement? Parent { get; set; }

    public string TypeName => $"List<{this.children[0].TypeName}>";

    public IEnumerator<ISchemaElement> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

internal class MapSchemaElement : ISchemaElement
{
    private readonly IReadOnlyDictionary<string, ISchemaElement> children;

    public MapSchemaElement(IReadOnlyDictionary<string, ISchemaElement> children)
    {
        this.children = children;
        foreach (var kvp in this.children)
        {
            kvp.Value.Parent = this;
        }
    }

    public ISchemaElement? Parent { get; set; }

    public string TypeName { get; private init; }

    public IEnumerator<ISchemaElement> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield break;
    }
}

internal class YamlVisitor : IYamlVisitor
{
    public ISchemaElement? Value { get; set; }
    
    public void Visit(YamlStream stream)
    {
        throw new NotImplementedException();
    }

    public void Visit(YamlDocument document)
    {
        document.RootNode.Accept(this);
    }

    public void Visit(YamlScalarNode scalar)
    {
        if (!string.IsNullOrEmpty(scalar.Value))
        {
            this.Value = new ScalarSchemaElement(scalar.Value!);
        }
    }

    public void Visit(YamlSequenceNode sequence)
    {
        var visitor = new YamlVisitor();
        List<ISchemaElement> elements = new();
        foreach (var item in sequence.Children)
        {
            visitor.Value = null;

            item.Accept(visitor);

            if (visitor.Value is not null)
            {
                elements.Add(visitor.Value);
            }
        }

        this.Value = new ListSchemaElement(elements);
    }

    public void Visit(YamlMappingNode mapping)
    {
        var visitor = new YamlVisitor();
        Dictionary<string, ISchemaElement> elements = new();
        foreach (var kvp in mapping.Children)
        {
            visitor.Value = null;
            
            if (kvp.Key is not YamlScalarNode scalarKey || string.IsNullOrEmpty(scalarKey.Value))
            {
                throw new ConfiguratorDotNetException("Mapipngs must have non-null string keys.");
            }

            kvp.Value.Accept(visitor);

            if (visitor.Value is null)
            {
                throw new ConfiguratorDotNetException("Mapping values may not be null.");
            }

            elements.Add(scalarKey.Value!, visitor.Value!);
        }

        this.Value = new MapSchemaElement(elements);
    }
}