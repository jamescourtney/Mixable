namespace ConfiguratorDotNet.Schema;

public class ScalarSchemaElementParser : ISchemaElementParser
{
    public bool CanParse(
        XElement node,
        MetadataAttributes metadataAttributes)
    {
        return !node.GetChildren().Any();
    }

    public SchemaElement Parse(
        SchemaElement? parent,
        XElement node,
        MetadataAttributes metadataAttributes,
        IErrorCollector errorCollector,
        ParseCallback parseChild)
    {
        List<XElement> children = node.GetChildren().ToList();

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
                    $"Unable to find parse '{node.Value}' as a '{scalarType.TypeName}'.",
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