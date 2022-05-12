namespace Mixable.Schema;

/// <summary>
/// Visits elements of a schema.
/// </summary>
public interface ISchemaVisitor
{
    /// <summary>
    /// Initializes the visitor based on the given metadata.
    /// </summary>
    void Run(SchemaElement element, DocumentMetadata metadata, IErrorCollector errorCollector);

    /// <summary>
    /// Indicates if the given visitor should process the document based upon the metadata.
    /// </summary>
    bool ShouldProcess(DocumentMetadata metadata);
}

/// <summary>
/// Visits elements of a schema.
/// </summary>
public interface ISchemaVisitor<T> : ISchemaVisitor
{
    /// <summary>
    /// Visits a list element.
    /// </summary>
    T Accept(ListSchemaElement list, IErrorCollector errorCollector);

    /// <summary>
    /// Visits a map element.
    /// </summary>
    T Accept(MapSchemaElement map, IErrorCollector errorCollector);

    /// <summary>
    /// Visits a scalar element.
    /// </summary>
    T Accept(ScalarSchemaElement scalar, IErrorCollector errorCollector);
}