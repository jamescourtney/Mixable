namespace Mixable.Schema;

internal class DerivedSchemaAttributeValidator : IAttributeValidator
{
    public MetadataAttributes Validate(XElement element, IErrorCollector errorCollector)
    {
        var attrs = MetadataAttributes.Extract(element, errorCollector);
        var path = element.GetDocumentPath();

        if (attrs.List is not null)
        {
            errorCollector.Error(
               $"Derived schemas may not have the {Constants.Attributes.List.LocalName} attribute defined.",
               path);
        }

        if (attrs.TypeName is not null)
        {
            errorCollector.Error(
                $"Derived schemas may not have the {Constants.Attributes.Type.LocalName} attribute defined.",
                path);
        }

        return attrs;
    }
}
