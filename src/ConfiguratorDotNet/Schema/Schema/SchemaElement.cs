namespace ConfiguratorDotNet.Schema;

/// <summary>
/// A generic element of an XML schema.
/// </summary>
public abstract class SchemaElement
{
    protected SchemaElement(SchemaElement? parent, XElement xElement)
    {
        this.Parent = parent;
        this.XmlElement = xElement;
    }

    /// <summary>
    /// Reference to the parent.
    /// </summary>
    public SchemaElement? Parent { get; }

    /// <summary>
    /// The XML element associated with this schema element. Contents may change as merge operations
    /// are applied.
    /// </summary>
    public XElement XmlElement { get; }

    /// <summary>
    /// Recursively merges the given element into this one. Assumes that 
    /// <see cref="MatchesSchema(XElement, IAttributeValidator, out string, out string)"/>
    /// has been invoked.
    /// </summary>
    public abstract void MergeWith(
        XElement element,
        IAttributeValidator validator);

    /// <summary>
    /// Recursively tests whether the given element matches the current schema.
    /// </summary>
    public abstract bool MatchesSchema(
        XElement element,
        IAttributeValidator validator,
        out string mismatchPath,
        out string error);

    public static bool operator ==(SchemaElement? a, SchemaElement? b)
    {
        bool aNull = a is null;
        bool bNull = b is null;

        if (aNull != bNull)
        {
            return false;
        }

        if (aNull)
        {
            return true;
        }

        return a!.Equals(b);
    }

    public static bool operator !=(SchemaElement? a, SchemaElement? b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not SchemaElement otherSchema)
        {
            return false;
        }

        return this.Equals(otherSchema);
    }

    public abstract bool Equals(SchemaElement? other);

    public override abstract int GetHashCode();

    public abstract T Accept<T>(ISchemaVisitor<T> visitor);
}