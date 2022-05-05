namespace ConfiguratorDotNet.Schema;

/// <summary>
/// A callback to parse an XElement.
/// </summary>
public delegate SchemaElement ParseCallback(
    SchemaElement? parent,
    XElement node);

/// <summary>
/// Parser implementation for a single type of Schema Element.
/// </summary>
public interface ISchemaElementParser
{
    /// <summary>
    /// Returns a value indicating if this <see cref="ISchemaElementParser"/> can
    /// handle the given <paramref name="node"/>.
    /// </summary>
    bool CanParse(XElement node, MetadataAttributes metadataAttributes);
    
    /// <summary>
    /// Parses the given <paramref name="node"/> as a Schema Element.
    /// </summary>
    SchemaElement Parse(
        SchemaElement? parent,
        XElement node,
        IAttributeValidator attributeValidator,
        IErrorCollector errorCollector,
        ParseCallback parseChild);
}