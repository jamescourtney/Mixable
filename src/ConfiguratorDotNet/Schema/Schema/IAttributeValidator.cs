namespace ConfiguratorDotNet.Schema;

public interface IAttributeValidator
{
    MetadataAttributes Validate(
        XElement element,
        IErrorCollector errorCollector);
}