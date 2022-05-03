namespace ConfiguratorDotNet.Schema;

internal interface IAttributeValidator
{
    bool TryValidate(XElement element, out string path, out string error, out MetadataAttributes attrs);
}

internal static class IAttributeValidatorExtensions
{
    public static MetadataAttributes Validate(this IAttributeValidator validator, XElement element)
    {
        if (!validator.TryValidate(element, out string path, out string error, out MetadataAttributes attrs))
        {
            throw new ConfiguratorDotNetException($"Attribute validation failed: {error}. Path = '{path}'.");
        }

        return attrs;
    }
}
