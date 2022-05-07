namespace ConfiguratorDotNet.Schema;

public class MapSchemaElementParser : ISchemaElementParser
{
    public bool CanParse(
        XElement node,
        MetadataAttributes metadataAttributes)
    {
        IEnumerable<XElement> children = node.GetFilteredChildren();

        if (!children.Any())
        {
            // No children => not a map
            return false;
        }

        int maxTagCount = children.GroupBy(x => x.Name).Select(x => x.Count()).Max();

        // No repeated children.
        return  maxTagCount == 1;
    }

    public SchemaElement Parse(
        XElement node,
        IAttributeValidator attributeValidator,
        IErrorCollector errorCollector,
        ParseCallback parseChild)
    {
        MapSchemaElement mapElement = new(node);

        foreach (var child in node.GetFilteredChildren())
        {
            mapElement.AddChild(
                parseChild(child),
                errorCollector);
        }

        return mapElement;
    }
}