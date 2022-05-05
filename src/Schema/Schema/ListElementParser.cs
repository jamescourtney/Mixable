namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Parses list schema elements.
/// </summary>
public class ListSchemaElementParser : ISchemaElementParser
{
    public bool CanParse(
        XElement node,
        MetadataAttributes metadataAttributes)
    {
        bool looksLikeList = false;

        if (metadataAttributes.List == true)
        {
            // User told us it's a list. Trust them.
            looksLikeList = true;
        }
        else if (node.GetChildren(Constants.Tags.ListTemplateTagName).Any())
        {
            // Had a template tag.
            looksLikeList = true;
        }
        else
        {
            List<XElement> children = node.GetFilteredChildren().ToList();
            int distinctTagNames = children.Select(x => x.Name).Distinct().Count();

            // A single tag name and at least two children means a list.
            looksLikeList = distinctTagNames == 1 && children.Count > 1;
        }

        // If it looks like a list so far, go to the trouble of making sure we can extract the template.
        if (looksLikeList)
        {
            NoOpErrorCollector ec = new();
            if (this.GetTemplateNode(node, ec) is null || ec.HasErrors)
            {
                looksLikeList = false;
            }
        }

        return looksLikeList;
    }

    public SchemaElement Parse(
        SchemaElement? parent,
        XElement node,
        MetadataAttributes metadataAttributes,
        IErrorCollector errorCollector,
        ParseCallback parseChild)
    {
        // Grab the template.
        XElement templateElement = this.GetTemplateNode(node, errorCollector)!; // Not null since we check up above.

        // Parse the template for structure.
        SchemaElement template = parseChild(parent, templateElement, errorCollector);

        // Make a new list based on the template.
        ListSchemaElement listElement = new(parent, node, template);

        // Ensure all the current children match the schema.
        listElement.MatchesSchema(node, MatchKind.Strict, AttributeValidator, errorCollector);

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
                    firstTemplate.GetDocumentPath());
            }

            var templateChildren  = firstTemplate.GetFilteredChildren();

            // Exactly one child.
            if (!templateChildren.Any() || templateChildren.Skip(1).Any())
            {
                errorCollector.Error(
                    "List templates must have exactly one child element.",
                    firstTemplate.GetDocumentPath());
            }

            return templateChildren.FirstOrDefault();
        }

        return node.GetFilteredChildren().FirstOrDefault();
    }
}