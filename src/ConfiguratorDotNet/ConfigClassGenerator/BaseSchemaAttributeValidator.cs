using ConfiguratorDotNet.Runtime;

namespace ConfiguratorDotNet.Generator;

internal class BaseSchemaAttributeValidator : IAttributeValidator
{
    public MetadataAttributes Validate(XElement element)
    {
        MetadataAttributes attributes = MetadataAttributes.Extract(element);

        if (attributes.ListMergePolicy is not null)
        {
            throw new ConfiguratorDotNetException("Base schemas may not have a merge policy defined.");
        }

        return attributes;
    }
}
