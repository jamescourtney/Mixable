namespace Mixable.Schema;

internal class IntermediateSchemaAttributeValidator : IAttributeValidator
{
    public IAttributeValidator RootValidator => this;

    public MetadataAttributes Validate(XElement element, IErrorCollector errorCollector)
    {
        var attrs = MetadataAttributes.Extract(element, errorCollector);

        ValidateNoRawTypeName(attrs, errorCollector, element);

        switch (attrs.Modifier)
        {
            case NodeModifier.None:
            case NodeModifier.Abstract:
            case NodeModifier.Final:
                break;

            default:
                errorCollector.Error(
                    $"Intermediate schemas may not use the {Constants.Attributes.Flags} attribute to set a node to {attrs.Modifier}.",
                    element.GetDocumentPath());
                break;

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
