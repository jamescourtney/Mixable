namespace Mixable.Schema;

/// <summary>
/// Visits elements of a schema.
/// </summary>
public interface ISchemaVisitor<T>
{
    /// <summary>
    /// Visits a list element.
    /// </summary>
    T Accept(ListSchemaElement list);

    /// <summary>
    /// Visits a map element.
    /// </summary>
    T Accept(MapSchemaElement map);

    /// <summary>
    /// Visits a scalar element.
    /// </summary>
    T Accept(ScalarSchemaElement scalar);
}