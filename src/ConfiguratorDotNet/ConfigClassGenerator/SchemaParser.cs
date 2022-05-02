using System.Linq;
using ConfiguratorDotNet.Runtime;

namespace ConfiguratorDotNet.Generator;

internal static class SchemaParser
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

    internal static SchemaElement Classify(SchemaElement? parent, XElement xElement, TypeNameStack stack, IAttributeValidator validator)
    {
        stack.Push(xElement.Name.LocalName);

        try
        {
            MetadataAttributes attrs = validator.Validate(xElement);
            List<XElement> children = xElement
                .GetChildren()
                .Where(x => x.Name.NamespaceName != Constants.XMLNamespace) // ignore our metadata namespace.
                .ToList();

            if (TryCreateScalarSchemaElement(attrs, xElement, parent, stack, children, out var schemaElement))
            {
                return schemaElement;
            }

            int distinctChildTagNames = children.Select(x => x.Name).Distinct().Count();
            int maxChildTagCount = children.GroupBy(x => x.Name).Select(g => g.Count()).Max();

            bool looksLikeList = distinctChildTagNames == 1 && children.Count > 1;
            bool looksLikeMap = distinctChildTagNames == children.Count;
            bool isList = looksLikeList;

            if (attrs.List is not null)
            {
                isList = attrs.List.Value;
            }

            if (isList)
            {
                if (distinctChildTagNames > 1)
                {
                    throw new ConfiguratorDotNetException($"List element '{stack}' had more than one distinct child name.");
                }

                ListSchemaElement listElement = new(parent, xElement);
                foreach (var child in children)
                {
                    listElement.AddChild(Classify(listElement, child, stack, validator));
                }

                return listElement;
            }
            else
            {
                if (maxChildTagCount > 1)
                {
                    throw new ConfiguratorDotNetException($"Map element '{stack}' had duplicate child tags.");
                }

                MapSchemaElement mapElement = new(parent, xElement) { TypeName = stack.ToString() };
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

    private static bool TryCreateScalarSchemaElement(
        MetadataAttributes attrs,
        XElement xElement,
        SchemaElement? parent,
        TypeNameStack stack,
        List<XElement> children,
        [NotNullWhen(true)] out SchemaElement? element)
    {
        element = null;

        if (attrs.CustomParser is not null)
        {
            // Custom logic for this subtree.
            if (attrs.TypeName is null)
            {
                throw new ConfiguratorDotNetException($"The '{Constants.Structure.TypeAttributeName}' attribute is required when using '{Constants.Structure.CustomParserAttributeName}'.");
            }

            element = new ScalarSchemaElement(attrs.TypeName, attrs.CustomParser, parent, xElement);
        }
        else if (children.Count == 0)
        {
            // Leaf node. Infer type or use type name attribute.
            if (string.IsNullOrEmpty(xElement.Value))
            {
                throw new ConfiguratorDotNetException("Unable to infer types for empty nodes.");
            }

            string typeName = InferType(xElement.Value);

            if (attrs.TypeName is not null)
            {
                typeName = attrs.TypeName;
            }

            element = new ScalarSchemaElement(typeName, null, parent, xElement);
        }
        
        return element is not null;
    }

    private static string InferType(string text)
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
