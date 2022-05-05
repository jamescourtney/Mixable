namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Validates metadata attributes.
/// </summary>
public interface IAttributeValidator
{
    /// <summary>
    /// Validates the given element's metadata attributes, returning the actual metadata.
    /// </summary>
    MetadataAttributes Validate(
        XElement element,
        IErrorCollector errorCollector);
}