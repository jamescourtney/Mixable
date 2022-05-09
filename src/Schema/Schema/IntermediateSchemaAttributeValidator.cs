namespace Mixable.Schema;

internal class IntermediateSchemaAttributeValidator : IAttributeValidator
{
    public MetadataAttributes Validate(XElement element, IErrorCollector errorCollector)
    {
        var attrs = MetadataAttributes.Extract(element, errorCollector);

        ValidateNoRawTypeName(attrs, errorCollector, element);

        if (attrs.Modifier is not null)
        {
            if (attrs.Modifier != NodeModifier.Abstract)
            {
                errorCollector.Error(
                    $"Intermediate schemas may only use the {Constants.Attributes.Flags} attribute to set a node to {nameof(NodeModifier.Abstract)}",
                    element.GetDocumentPath());
            }
        }

        return attrs;
    }

    internal static void ValidateNoRawTypeName(MetadataAttributes attrs, IErrorCollector errorCollector, XElement element)
    {
        if (attrs.RawTypeName is not null)
        {
            errorCollector.Error(
                $"Derived schemas may not have the {Constants.Attributes.Type.LocalName} attribute defined.",
                element.GetDocumentPath());
        }
    }
}
