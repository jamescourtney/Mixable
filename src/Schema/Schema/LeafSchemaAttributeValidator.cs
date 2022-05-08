namespace Mixable.Schema;

internal class LeafSchemaAttributeValidator : IAttributeValidator
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
            errorCollector.Error(
                $"Leaf schemas may not use the Mixable {Constants.Attributes.Flags.LocalName} attribute.",
                path);
        }

        return attrs;
    }
}
