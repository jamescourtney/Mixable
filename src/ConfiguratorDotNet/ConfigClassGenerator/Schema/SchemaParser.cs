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
                .GetFilteredChildren()
                .ToList();

            if (TryCreateScalarSchemaElement(attrs, xElement, parent, children, out var schemaElement))
            {
                return schemaElement;
            }

            int distinctChildTagNames = children.Select(x => x.Name).Distinct().Count();

            // If the user said it's a list or it just looks like a list.
            bool isList = attrs.List ?? (distinctChildTagNames == 1 && children.Count > 1);

            if (isList)
            {
                TryCreateListSchemaElement(xElement, parent, stack, validator, out ListSchemaElement? listElement);
                return listElement;
            }
            else
            {
                int maxChildTagCount = children.GroupBy(x => x.Name).Select(g => g.Count()).Max();
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

    internal static bool TryCreateListSchemaElement(
        XElement xElement,
        SchemaElement? parent,
        TypeNameStack stack,
        IAttributeValidator validator,
        [NotNullWhen(true)] out ListSchemaElement? element)
    {
        List<XElement> children = xElement.GetFilteredChildren().ToList();
        int distinctChildTagNames = children.Select(x => x.Name).Distinct().Count();
        
        if (distinctChildTagNames > 1)
        {
            throw new ConfiguratorDotNetException($"List element '{xElement.GetDocumentPath()}' had more than one distinct child name.");
        }

        ListSchemaElement listElement = new(parent, xElement, children[0].Name);
        foreach (var child in children)
        {
            listElement.AddChild(Classify(listElement, child, stack, validator));
        }

        element =  listElement;
        return true;
    }

    internal static bool TryCreateScalarSchemaElement(
        MetadataAttributes attrs,
        XElement xElement,
        SchemaElement? parent,
        List<XElement> children,
        [NotNullWhen(true)] out SchemaElement? element)
    {
        element = null;

        if (children.Count == 0)
        {
            // Leaf node. Infer type or use type name attribute.
            if (string.IsNullOrEmpty(xElement.Value))
            {
                throw new ConfiguratorDotNetException($"Unable to infer types for empty nodes. Path = '{parent?.XPath}'.");
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
