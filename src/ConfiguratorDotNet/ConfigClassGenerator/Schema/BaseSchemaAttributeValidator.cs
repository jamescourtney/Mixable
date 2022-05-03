namespace ConfiguratorDotNet.Generator;

internal class BaseSchemaAttributeValidator : IAttributeValidator
{
    public bool TryValidate(XElement element, out string path, out string error, out MetadataAttributes attrs)
    {
        attrs = MetadataAttributes.Extract(element);

        if (attrs.ListMergePolicy is not null)
        {
            error = "Base schemas may not have a merge policy defined.";
            path = element.GetDocumentPath();
            return false;
        }

        error = string.Empty;
        path = string.Empty;
        return true;
    }
}
