namespace Mixable.Schema;

public enum MatchKind
{
    Subset = 0,
    Strict = 1,
}

/// <summary>
/// A generic element of an XML schema.
/// </summary>
public abstract class SchemaElement
{
    protected SchemaElement(XElement xElement)
    {
        this.XmlElement = xElement;
        this.Optional = MetadataAttributes.Extract(xElement, null).Optional;
    }

    /// <summary>
    /// The XML element associated with this schema element. Contents may change as merge operations
    /// are applied.
    /// </summary>
    public XElement XmlElement { get; }

    /// <summary>
    /// Indicates if this element has the Optional attribute.
    /// </summary>
    public bool Optional { get; }

    /// <summary>
    /// Merges the given element with the schema, assuming it passes validation.
    /// </summary>
    public bool MergeWith(XElement element, IErrorCollector? collector)
    {
        collector ??= new NoOpErrorCollector();

        IAttributeValidator validator = new DerivedSchemaAttributeValidator();
        if (!this.MatchesSchema(element, MatchKind.Subset, validator, collector))
        {
            return false;
        }

        this.MergeWithProtected(element, validator, collector);
        return !collector.HasErrors;
    }

    /// <summary>
    /// Recursively merges the given element into this one. Assumes that 
    /// <see cref="MatchesSchema(XElement, IAttributeValidator, out string, out string)"/>
    /// has been invoked.
    /// </summary>
    protected internal abstract void MergeWithProtected(
        XElement element,
        IAttributeValidator validator,
        IErrorCollector errorCollector);

    /// <summary>
    /// Recursively tests whether the given element matches the current schema.
    /// </summary>
    protected internal abstract bool MatchesSchema(
        XElement element,
        MatchKind matchKind,
        IAttributeValidator validator,
        IErrorCollector errorCollector);

    public abstract T Accept<T>(ISchemaVisitor<T> visitor);
}