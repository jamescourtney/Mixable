namespace Mixable.Schema;

internal class BaseSchemaAttributeValidator : IAttributeValidator
{
    public MetadataAttributes Validate(XElement element, IErrorCollector errorCollector)
    {
        var attrs = MetadataAttributes.Extract(element, errorCollector);

        if (attrs.ListMergePolicy is not null)
        {
            errorCollector.Error("Base schemas may not have a ListMerge policy defined.", element.GetDocumentPath());
        }

        return attrs;
    }
}
