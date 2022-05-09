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
        metadataAttributes.EnsureNotAbstractOrFinal(errorCollector, "Map");

        MixableInternal.Assert(
            metadataAttributes.WellKnownType is null or WellKnownType.Map,
            "Expecting null or map");

        MapSchemaElement mapElement = new(node);
        var childValidator = GetMapAttributeValidator(attributeValidator);

        foreach (var child in node.GetFilteredChildren())
        {
            mapElement.AddChild(
                parseChild(child, childValidator),
                errorCollector);
        }

        return mapElement;
    }

    internal static IAttributeValidator GetMapAttributeValidator(IAttributeValidator validator)
    {
        return validator.DecorateWith((attrs, errorCollector) =>
        {
            if (attrs.Modifier == NodeModifier.Optional)
            {
                errorCollector.Error($"Map elements may not use the {NodeModifier.Optional} modifier.", attrs.SourceElement);
            }

            return true; // continue processing later rules.
        });
    }
}