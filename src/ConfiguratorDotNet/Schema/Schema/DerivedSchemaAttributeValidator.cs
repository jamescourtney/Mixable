namespace ConfiguratorDotNet.Schema;

internal class DerivedSchemaAttributeValidator : IAttributeValidator
{
    public MetadataAttributes Validate(XElement element, IErrorCollector errorCollector)
    {
        var attrs = MetadataAttributes.Extract(element);
        var path = element.GetDocumentPath();

        if (attrs.List is not null)
        {
            errorCollector.Error(
               $"Derived schemas may not have the {Constants.Structure.ListAttributeName.LocalName} attribute defined.",
               path);
        }

        if (attrs.TypeName is not null)
        {
            errorCollector.Error(
                $"Derived schemas may not have the {Constants.Structure.TypeAttributeName.LocalName} attribute defined.",
                path);
        }

        return attrs;
    }
}
