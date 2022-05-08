namespace Mixable.Schema;

public class MapSchemaElementParser : ISchemaElementParser
{
    public bool SupportsUnparsableWellKnownTypes => false;

    public bool SupportsType(WellKnownType type)
    {
        return type == WellKnownType.Map;
    }

    public bool CanParse(
        XElement node,
        MetadataAttributes metadataAttributes)
    {
        MixableInternal.Assert(
            metadataAttributes.WellKnownType is null or WellKnownType.Map,
            "Expecting null or map");

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
        var metadataAttributes = attributeValidator.Validate(node, errorCollector);

        MixableInternal.Assert(
            metadataAttributes.WellKnownType is null or WellKnownType.Map,
            "Expecting null or map");

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