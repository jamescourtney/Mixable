namespace Mixable.Schema;

/// <summary>
/// Parses list schema elements.
/// </summary>
public class ListSchemaElementParser : ISchemaElementParser
{
    public bool SupportsUnparsableWellKnownTypes => false;

    public bool SupportsType(WellKnownType type)
    {
        return type == WellKnownType.List;
    }

    public bool CanParse(
        XElement node,
        MetadataAttributes metadataAttributes)
    {
        MixableInternal.Assert(
            metadataAttributes.WellKnownType is null or WellKnownType.List,
            "Expecting null or list");

        // User said it's a list OR there's a list template child.
        if (metadataAttributes.WellKnownType == WellKnownType.List ||
            node.GetChildren(Constants.Tags.ListTemplateTagName).Any())
        {
            return true;
        }

        List<XElement> children = node.GetFilteredChildren().ToList();
        int distinctTagNames = children.Select(x => x.Name).Distinct().Count();

        // A single tag name and at least two children means a list.
        if (distinctTagNames == 1 && children.Count > 1)
        {
            NoOpErrorCollector ec = new();
            return this.GetTemplateNode(node, ec) is not null && !ec.HasErrors;
        }

        return false;
    }

    public SchemaElement Parse(
        XElement node,
        IAttributeValidator attributeValidator,
        IErrorCollector errorCollector,
        ParseCallback parseChild)
    {
        var metadataAttributes = attributeValidator.Validate(node, errorCollector);

        MixableInternal.Assert(
            metadataAttributes.WellKnownType is null or WellKnownType.List,
            "Expecting null or list");

        // Grab the template.
        XElement? templateElement = this.GetTemplateNode(node, errorCollector)!; // Not null since we check up above.

        SchemaElement template;
        if (templateElement is not null)
        {
            // Parse the template for structure.
            template = parseChild(templateElement, GetListTemplateAttributeValidator(attributeValidator));
        }
        else
        {
            errorCollector.Error(
                "Couldn't determine type of list item. Lists must include at least one represenative node or a Template element.",
                node.GetDocumentPath());

            // Treat as map for the moment.
            template = new MapSchemaElement(node);
        }

        // Make a new list based on the template.
        ListSchemaElement listElement = new(node, template);

        // Ensure all the current children match the schema.
        listElement.MatchesSchema(node, MatchKind.Strict, attributeValidator, errorCollector);

        // If we are abstract, remove all the children.
        if (metadataAttributes.Modifier == NodeModifier.Abstract)
        {
            listElement.XmlElement.RemoveNodes();
        }

        return listElement;
    }

    internal static IAttributeValidator GetListTemplateAttributeValidator(IAttributeValidator validator)
    {
        // Use root validator as we want only "list" rules to apply for parsing our template. 
        // otherwise, other decorators may detect incorrect errors, such as use of "Optional".
        return validator.RootValidator.DecorateWith(
            (attributes, errorCollector) =>
            {
                switch (attributes.Modifier)
                {
                    case NodeModifier.Abstract:
                    case NodeModifier.Final:
                        errorCollector.Error($"Items within a list template may not be marked as '{attributes.Modifier}'.", attributes.SourceElement);
                        break;
                }

                return false; // stop processing additional rules.
            });
    }

    private XElement? GetTemplateNode(XElement node, IErrorCollector errorCollector)
    {
        var templateNodes = node.GetChildren(Constants.Tags.ListTemplateTagName);

        if (templateNodes.Any())
        {
            XElement firstTemplate = templateNodes.First();

            if (templateNodes.Skip(1).Any())
            {
                errorCollector.Error(
                    "Lists may only have a single template node.",
                    node.GetDocumentPath());
            }

            var templateChildren  = firstTemplate.GetFilteredChildren();

            // Exactly one child.
            if (!templateChildren.Any() || templateChildren.Skip(1).Any())
            {
                errorCollector.Error(
                    "List templates must have exactly one child element.",
                    node.GetDocumentPath());
            }

            return templateChildren.FirstOrDefault();
        }

        return node.GetFilteredChildren().FirstOrDefault();
    }
}