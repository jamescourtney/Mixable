namespace Mixable.Schema;

internal class IntermediateSchemaAttributeValidator : IAttributeValidator
{
    public MetadataAttributes Validate(XElement element, IErrorCollector errorCollector)
    {
        var attrs = MetadataAttributes.Extract(element, errorCollector);
        var path = element.GetDocumentPath();

        if (attrs.RawTypeName is not null)
        {
            errorCollector.Error(
                $"Derived schemas may not have the {Constants.Attributes.Type.LocalName} attribute defined.",
                path);
        }


        if (attrs.Modifier is not null)
        {
            if (attrs.Modifier != NodeModifier.Abstract)
            {
                errorCollector.Error(
                    $"Intermediate schemas may only use the {Constants.Attributes.Flags} attribute to set a node to {nameof(NodeModifier.Abstract)}",
                    path);
            }
        }

        return attrs;
    }
}
