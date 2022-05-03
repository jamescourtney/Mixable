namespace ConfiguratorDotNet.Generator;

internal class DerivedSchemaAttributeValidator : IAttributeValidator
{
    public bool TryValidate(XElement element, out string path, out string error, out MetadataAttributes attrs)
    {
        attrs = MetadataAttributes.Extract(element);
        path = element.GetDocumentPath();

        if (attrs.List is not null)
        {
            error = "Derived schemas may not have the List attribute defined.";
            return false;
        }

        if (attrs.TypeName is not null)
        {
            error = "Derived schemas may not have the TypeName attribute defined.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
