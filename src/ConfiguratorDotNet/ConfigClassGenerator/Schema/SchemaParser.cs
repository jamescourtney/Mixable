﻿namespace ConfiguratorDotNet.Generator;

internal static class SchemaParser
{
    internal static SchemaElement Parse(XDocument document)
    {
        XmlMetadata data = new XmlMetadata(document);

        IAttributeValidator validator = new BaseSchemaAttributeValidator();
        if (string.IsNullOrEmpty(data.BaseFileName))
        {
            validator = new BaseSchemaAttributeValidator();
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

            if (children.Count == 0)
            {
                return CreateScalarElement(attrs, xElement, parent, children);
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

        SchemaElement template = Classify(parent, children[0], stack, validator);
        ListSchemaElement listElement = new(parent, xElement, template);

        foreach (var child in children)
        {
            if (!listElement.MatchesSchema(xElement, validator, out string errorXPath, out string error))
            {
                throw new ConfiguratorDotNetException($"List itemp child does not match template. Error = {error}, Path = {errorXPath}");
            }
        }

        element =  listElement;
        return true;
    }

    internal static ScalarSchemaElement CreateScalarElement(
        MetadataAttributes attrs,
        XElement xElement,
        SchemaElement? parent,
        List<XElement> children)
    {
        if (children.Count == 0)
        {
            ScalarType? scalarType;
            if (attrs.TypeName is null)
            {
                scalarType = ScalarType.GetInferredScalarType(xElement.Value);
            }
            else if (!ScalarType.TryGetExplicitScalarType(attrs.TypeName, out scalarType))
            {
                throw new ConfiguratorDotNetException($"Unable to find explicit scalar type '{attrs.TypeName}'. Path = '{xElement.GetDocumentPath()}'.");
            }

            if (!scalarType.Parser.CanParse(xElement.Value))
            {
                throw new ConfiguratorDotNetException($"Unable to find parse '{xElement.Value}' as a '{scalarType.TypeName}'. Path = '{xElement.GetDocumentPath()}'.");
            }

            return new ScalarSchemaElement(scalarType, parent, xElement);
        }
        else
        {
            throw new ConfiguratorDotNetException("Can't create scalar schema node when it has children");
        }
    }
}
