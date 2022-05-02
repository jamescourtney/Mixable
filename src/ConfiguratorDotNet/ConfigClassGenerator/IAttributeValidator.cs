namespace ConfiguratorDotNet.Generator;

internal interface IAttributeValidator
{
    MetadataAttributes Validate(XElement element);
}
