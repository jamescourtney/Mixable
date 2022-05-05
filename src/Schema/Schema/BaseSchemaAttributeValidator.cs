namespace ConfiguratorDotNet.Schema;

internal class BaseSchemaAttributeValidator : IAttributeValidator
{
    public MetadataAttributes Validate(XElement element, IErrorCollector errorCollector)
    {
        var attrs = MetadataAttributes.Extract(element);

        if (attrs.ListMergePolicy is not null)
        {
            errorCollector.Error("Base schemas may not have a merge policy defined.", element.GetDocumentPath());
        }

        return attrs;
    }
}
