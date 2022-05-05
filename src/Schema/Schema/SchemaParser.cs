namespace ConfiguratorDotNet.Schema;

public static class SchemaParser
{
    private static readonly BaseSchemaAttributeValidator AttributeValidator = new();

    public static bool TryParse(
        XDocument document,
        IErrorCollector? errorCollector,
        [NotNullWhen(true)] out SchemaElement? element)
    {
        errorCollector ??= new NoOpErrorCollector();
        element = null;

        if (document.Root is null)
        {
            errorCollector.Error("XML Document did not have a root element.");
            return false;
        }

        if (!DocumentMetadata.TryCreateFromXDocument(document, errorCollector, out _))
        {
            return false;
        }

        element = Classify(null, document.Root, errorCollector);
        return true;
    }

    private static SchemaElement Classify(
        SchemaElement? parent,
        XElement xElement,
        IErrorCollector errorCollector)
    {
        MetadataAttributes attrs = AttributeValidator.Validate(xElement, errorCollector);

        List<XElement> children = xElement
            .GetFilteredChildren()
            .ToList();

        if (children.Count == 0)
        {
            return CreateScalarElement(attrs, xElement, parent, children, errorCollector);
        }

        int distinctChildTagNames = children.Select(x => x.Name).Distinct().Count();

        bool hasListTemplateChild = xElement.GetChildren().Any(x => x.Name == Constants.Tags.ListTemplateTagName);

        bool isList = attrs.List
            ?? (hasListTemplateChild || (distinctChildTagNames == 1 && children.Count > 1));

        if (isList)
        {
            return CreateListSchemaElement(xElement, parent, errorCollector);
        }
        else
        {
            int maxChildTagCount = children.GroupBy(x => x.Name).Select(g => g.Count()).Max();
            if (maxChildTagCount > 1)
            {
                errorCollector.Error(
                    $"Map element had duplicate child tags.",
                    xElement.GetDocumentPath());
            }

            MapSchemaElement mapElement = new(parent, xElement);
            foreach (var child in children)
            {
                mapElement.AddChild(
                    Classify(mapElement, child, errorCollector),
                    errorCollector);
            }

            return mapElement;
        }
    }

    internal static ListSchemaElement CreateListSchemaElement(
        XElement xElement,
        SchemaElement? parent,
        IErrorCollector errorCollector)
    {
        List<XElement> children = xElement.GetFilteredChildren().ToList();
        int distinctChildTagNames = children.Select(x => x.Name).Distinct().Count();
        
        if (distinctChildTagNames > 1)
        {
            errorCollector.Error(
                "List element has more than one distinct child tag name.",
                xElement.GetDocumentPath());
        }

        XElement templateElement = children[0];

        // See if there is a template node, and use it.
        {
            List<XElement> templateNodes = xElement.GetChildren(Constants.Tags.ListTemplateTagName).ToList();
            if (templateNodes.Count > 0)
            {
                if (templateNodes.Count != 1)
                {
                    errorCollector.Error("Lists may only have a single template node.", templateNodes[0].GetDocumentPath());
                }

                List<XElement> templateChildren = templateNodes[0].GetFilteredChildren().ToList();
                if (templateChildren.Count != 1)
                {
                    errorCollector.Error("List templates must have a single child.", templateNodes[0].GetDocumentPath());
                }

                templateElement = templateChildren[0];
            }
        }

        SchemaElement template = Classify(parent, templateElement, errorCollector);
        ListSchemaElement listElement = new(parent, xElement, template);

        listElement.MatchesSchema(xElement, MatchKind.Strict, AttributeValidator, errorCollector);

        return listElement;
    }

    internal static ScalarSchemaElement CreateScalarElement(
        MetadataAttributes attrs,
        XElement xElement,
        SchemaElement? parent,
        List<XElement> children,
        IErrorCollector errorCollector)
    {
        ScalarType? scalarType = null;

        if (children.Count == 0)
        {
            if (attrs.TypeName is null)
            {
                scalarType = ScalarType.GetInferredScalarType(xElement.Value);
            }
            else if (!ScalarType.TryGetExplicitScalarType(attrs.TypeName, out scalarType))
            {
                errorCollector.Error(
                    $"Unable to find explicit scalar type '{attrs.TypeName}'.",
                    xElement.GetDocumentPath());
            }

            if (scalarType is not null)
            {
                if (!scalarType.Parser.CanParse(xElement.Value))
                {
                    errorCollector.Error(
                        $"Unable to find parse '{xElement.Value}' as a '{scalarType.TypeName}'.",
                        xElement.GetDocumentPath());
                }
            }
        }
        else
        {
            errorCollector.Error(
                "Can't create scalar schema node when it has children",
                xElement.GetDocumentPath());
        }

        return new ScalarSchemaElement(scalarType ?? ScalarType.String, parent, xElement);
    }
}
