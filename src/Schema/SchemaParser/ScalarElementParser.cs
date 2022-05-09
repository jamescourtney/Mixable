namespace Mixable.Schema;

public class ScalarSchemaElementParser : ISchemaElementParser
{
    public bool SupportsUnparsableWellKnownTypes => false;

    public bool SupportsType(WellKnownType type)
    {
        return ScalarType.TryGetExplicitScalarType(type, out _);
    }

    public bool CanParse(
        XElement node,
        MetadataAttributes metadataAttributes)
    {
        if (node.GetChildren().Any())
        {
            // We don't deal with children!
            return false;
        }

        if (metadataAttributes.WellKnownType is not null)
        {
            // If the type is specified, make sure it's a scalar before we commit
            // to it.
            return ScalarType.TryGetExplicitScalarType(metadataAttributes.WellKnownType.Value, out _);
        }

        // Otherwise, it's a node without children or an explicit type.
        // it's a good bet that it's a a scalar.
        return true;
    }

    public SchemaElement Parse(
        XElement node,
        IAttributeValidator attributeValidator,
        IErrorCollector errorCollector,
        ParseCallback parseChild)
    {
        MetadataAttributes metadataAttributes = attributeValidator.Validate(node, errorCollector);

        ScalarType? scalarType;

        if (metadataAttributes.WellKnownType is null)
        {
            scalarType = ScalarType.GetInferredScalarType(node.Value);
        }
        else
        {
            MixableInternal.Assert(
                 ScalarType.TryGetExplicitScalarType(metadataAttributes.WellKnownType.Value, out scalarType),
                 "Couldn't find scalar type for: " + metadataAttributes.WellKnownType);
        }

        if (!scalarType.Parser.CanParse(node.Value))
        {
            errorCollector.Error(
                $"Unable to parse '{node.Value}' as a '{metadataAttributes.WellKnownType}'.",
                node);
        }

        return new ScalarSchemaElement(
            scalarType,
            node);
    }
}