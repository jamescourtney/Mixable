﻿namespace ConfiguratorDotNet.Schema;

public static class SchemaParser
{
    public static SchemaElement Parse(XDocument document)
    {
        if (document.Root is null)
        {
            throw new System.IO.InvalidDataException("XML document did not have a root element.");
        }

        DocumentMetadata data = new DocumentMetadata(document.Root);
        if (!data.ValidateAsTemplateFile(out string? error))
        {
            throw new ConfiguratorDotNetException(error);
        }

        IAttributeValidator validator = new BaseSchemaAttributeValidator();
        return Classify(null, document.Root, validator);
    }

    internal static SchemaElement Classify(SchemaElement? parent, XElement xElement, IAttributeValidator validator)
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
            return CreateListSchemaElement(xElement, parent, validator);
        }
        else
        {
            int maxChildTagCount = children.GroupBy(x => x.Name).Select(g => g.Count()).Max();
            if (maxChildTagCount > 1)
            {
                throw new ConfiguratorDotNetException($"Map element '{xElement.GetDocumentPath()}' had duplicate child tags.");
            }

            MapSchemaElement mapElement = new(parent, xElement);
            foreach (var child in children)
            {
                mapElement.AddChild(
                    child.Name.LocalName,
                    Classify(mapElement, child, validator));
            }

            return mapElement;
        }
    }

    internal static ListSchemaElement CreateListSchemaElement(
        XElement xElement,
        SchemaElement? parent,
        IAttributeValidator validator)
    {
        List<XElement> children = xElement.GetFilteredChildren().ToList();
        int distinctChildTagNames = children.Select(x => x.Name).Distinct().Count();
        
        if (distinctChildTagNames > 1)
        {
            throw new ConfiguratorDotNetException($"List element '{xElement.GetDocumentPath()}' had more than one distinct child name.");
        }

        SchemaElement template = Classify(parent, children[0], validator);
        ListSchemaElement listElement = new(parent, xElement, template);

        foreach (var child in children)
        {
            if (!listElement.MatchesSchema(xElement, validator, out string errorXPath, out string error))
            {
                throw new ConfiguratorDotNetException($"List item child does not match template. Error = {error}, Path = {errorXPath}");
            }
        }

        return listElement;
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
