namespace ConfiguratorDotNet.Schema;

/// <summary>
/// Visits elements of a schema.
/// </summary>
public interface ISchemaVisitor
{
    /// <summary>
    /// Visits a list element.
    /// </summary>
    void Accept(ListSchemaElement list);

    /// <summary>
    /// Visits a map element.
    /// </summary>
    void Accept(MapSchemaElement map);

    /// <summary>
    /// Visits a scalar element.
    /// </summary>
    void Accept(ScalarSchemaElement scalar);
}