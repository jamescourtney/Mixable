namespace ConfiguratorDotNet.Schema;

public class ScalarSchemaElementParser : ISchemaElementParser
{
    public bool CanParse(
        XElement node,
        MetadataAttributes metadataAttributes)
    {
        if (node.GetChildren().Any())
        {
            // We don't deal with children!
            return false;
        }

        if (metadataAttributes.TypeName is not null
         && !ScalarType.TryGetExplicitScalarType(metadataAttributes.TypeName, out _))
        {
            // Type specified but we don't know about it => we can't parse it.
            return false;
        }

        return true;
    }

    public SchemaElement Parse(
        SchemaElement? parent,
        XElement node,
        IAttributeValidator attributeValidator,
        IErrorCollector errorCollector,
        ParseCallback parseChild)
    {
        List<XElement> children = node.GetChildren().ToList();
        MetadataAttributes metadataAttributes = attributeValidator.Validate(node, errorCollector);

        ScalarType? scalarType;
        if (metadataAttributes.TypeName is null)
        {
            scalarType = ScalarType.GetInferredScalarType(node.Value);
        }
        else if (!ScalarType.TryGetExplicitScalarType(metadataAttributes.TypeName, out scalarType))
        {
            errorCollector.Error(
                $"Unable to find explicit scalar type '{metadataAttributes.TypeName}'.",
                node.GetDocumentPath());
        }

        if (scalarType is not null)
        {
            if (!scalarType.Parser.CanParse(node.Value))
            {
                errorCollector.Error(
                    $"Unable to parse '{node.Value}' as a '{scalarType.TypeName}'.",
                    node.GetDocumentPath());
            }
        }

        // use string if we hit an error inferring the type. This allows everything to succeed
        // and more errors to be collected.
        return new ScalarSchemaElement(
            scalarType ?? ScalarType.String,
            parent,
            node);
    }
}