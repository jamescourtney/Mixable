namespace Mixable.Schema;

internal class LeafSchemaAttributeValidator : IAttributeValidator
{
    public IAttributeValidator RootValidator => this;

    public MetadataAttributes Validate(XElement element, IErrorCollector errorCollector)
    {
        var attrs = MetadataAttributes.Extract(element, errorCollector);
        IntermediateSchemaAttributeValidator.ValidateNoRawTypeName(attrs, errorCollector, element);

        if (attrs.Modifier != NodeModifier.None)
        {
            errorCollector.Error(
                $"Leaf schemas may not use the Mixable '{Constants.Attributes.Flags.LocalName}' attribute.",
                element.GetDocumentPath());
        }

        return attrs;
    }
}
