using System.IO;
using System.Linq;
using System.Text;
using ConfiguratorDotNet.Runtime;

namespace ConfiguratorDotNet.Generator;

internal abstract class SchemaElement
{
    protected SchemaElement(SchemaElement? parent)
    {
        this.Parent = parent;
    }

    /// <summary>
    /// Gets or sets the type name for the current schema element.
    /// </summary>
    public string? TypeName { get; set; }

    public SchemaElement? Parent { get; }

    public virtual IEnumerable<SchemaElement> Children => Array.Empty<SchemaElement>();
}

internal enum ListMergePolicy
{
    Concatenate = 0,
    Replace = 1,
}

internal static class ElementClassifier
{
    internal static SchemaElement Parse(XDocument document)
    {
        XmlMetadata data = new XmlMetadata(document);

        IAttributeValidator validator;
        if (string.IsNullOrEmpty(data.BaseFileName))
        {
            validator = new BaseSchemaAttributeValidator();
        }
        else
        {
            validator = new DerivedSchemaAttributeValidator();
        }

        return Classify(null, document.Root, new TypeNameStack(), validator);
    }

    internal static SchemaElement Classify(SchemaElement? parent, XElement element, TypeNameStack stack, IAttributeValidator validator)
    {
        stack.Push(element.Name.LocalName);

        try
        {
            MetadataAttributes attrs = validator.Validate(element);

            // Custom logic for this subtree.
            if (attrs.CustomParser is not null)
            {
                if (attrs.TypeName is null)
                {
                    throw new ConfiguratorDotNetException($"The '{Constants.Structure.TypeAttributeName}' attribute is required when using '{Constants.Structure.CustomParserAttributeName}'.");
                }

                return new ScalarSchemaElement(attrs.TypeName, attrs.CustomParser, parent);
            }

            List<XElement> children = element.GetChildren().Where(x => x.Name.NamespaceName != Constants.XMLNamespace).ToList();

            // Leaf node. Infer type or use type name attribute.
            if (children.Count == 0)
            {
                if (string.IsNullOrEmpty(element.Value))
                {
                    throw new ConfiguratorDotNetException("Unable to infer types for empty nodes.");
                }

                string typeName = InferType(element.Value);

                if (attrs.TypeName is not null)
                {
                    typeName = attrs.TypeName;
                }

                return new ScalarSchemaElement(typeName, null, parent);
            }

            int distinctChildTagNames = children.Select(x => x.Name).Distinct().Count();
            bool isList = false;

            if (distinctChildTagNames == 1 && children.Count == 1)
            {
                if (attrs.List is null)
                {
                    throw new ConfiguratorDotNetException($"Element '{stack}' was ambiguous. It can be either a list or a map. Use the '{Constants.Structure.ListAttributeName}' attribute to distinguish.");
                }

                isList = attrs.List.Value;
            }
            else if (distinctChildTagNames == 1)
            {
                isList = true;
            }

            if (isList)
            {
                ListSchemaElement listElement = new(parent);
                foreach (var child in children)
                {
                    listElement.AddChild(Classify(listElement, child, stack, validator));
                }

                return listElement;
            }
            else
            {
                MapSchemaElement mapElement = new(parent) { TypeName = stack.ToString() };
                foreach (var child in children)
                {
                    mapElement.AddChild(
                        child.Name.LocalName,
                        Classify(mapElement, child, stack, validator));
                }

                return mapElement;
            }
        }
        finally
        {
            stack.Pop();
        }
    }

    internal static string InferType(string text)
    {
        if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) || 
            string.Equals(text, "false", StringComparison.OrdinalIgnoreCase))
        {
            return "bool";
        }

        if (int.TryParse(text, out _))
        {
            return "int";
        }

        if (double.TryParse(text, out _))
        {
            return "double";
        }

        return "string";
    }
}

internal class ScalarSchemaElement : SchemaElement
{
    public ScalarSchemaElement(string typeName, string? customParser, SchemaElement? parent)
        : base(parent)
    {
        this.TypeName = typeName;
        this.CustomParser = customParser;
    }

    public string? CustomParser { get; set; }
}

internal class ListSchemaElement : SchemaElement
{
    private readonly List<SchemaElement> children = new();

    public ListSchemaElement(SchemaElement? parent) : base(parent)
    {
    }

    public override IEnumerable<SchemaElement> Children => this.children;

    public void AddChild(SchemaElement element)
    {
        this.TypeName = $"List<{element.TypeName}>";
        this.children.Add(element);
    }
}

internal class MapSchemaElement : SchemaElement
{
    private readonly Dictionary<string, SchemaElement> children = new();

    public MapSchemaElement(SchemaElement? parent) : base(parent)
    {
    }

    public override IEnumerable<SchemaElement> Children => this.children.Values;

    public void AddChild(string tagName, SchemaElement child)
    {
        this.children.Add(tagName, child);
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