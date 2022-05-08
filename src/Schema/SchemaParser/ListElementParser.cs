namespace Mixable.Schema;

/// <summary>
/// Parses list schema elements.
/// </summary>
public class ListSchemaElementParser : ISchemaElementParser
{
    public bool CanParse(
        XElement node,
        MetadataAttributes metadataAttributes)
    {
        // User said it's a list OR there's a list template child.
        if (metadataAttributes.List == true ||
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
        // Grab the template.
        XElement? templateElement = this.GetTemplateNode(node, errorCollector)!; // Not null since we check up above.

        SchemaElement template;
        if (templateElement is not null)
        {
            // Parse the template for structure.
            template = parseChild(templateElement);
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

        return listElement;
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