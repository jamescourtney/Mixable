using ConfiguratorDotNet.Runtime;

namespace ConfiguratorDotNet.Generator;

internal class DerivedSchemaAttributeValidator : IAttributeValidator
{
    public MetadataAttributes Validate(XElement element)
    {
        MetadataAttributes attributes = MetadataAttributes.Extract(element);

        if (attributes.List is not null)
        {
            throw new ConfiguratorDotNetException("Derived schemas may not have the List attribute defined.");
        }

        if (attributes.TypeName is not null)
        {
            throw new ConfiguratorDotNetException("Derived schemas may not have the TypeName attribute defined.");
        }

        return attributes;
    }
}
