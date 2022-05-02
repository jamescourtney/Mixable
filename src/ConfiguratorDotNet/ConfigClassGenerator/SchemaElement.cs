﻿namespace ConfiguratorDotNet.Generator;

internal abstract class SchemaElement
{
    protected readonly XElement xElement;

    protected SchemaElement(SchemaElement? parent, XElement xElement)
    {
        this.Parent = parent;
        this.xElement = xElement;
    }

    /// <summary>
    /// Gets or sets the type name for the current schema element.
    /// </summary>
    public string? TypeName { get; set; }

    public string XPath => $"{this.Parent?.XPath}/{this.xElement.Name}";

    public SchemaElement? Parent { get; }

    public virtual IEnumerable<SchemaElement> Children => Array.Empty<SchemaElement>();

    public abstract void Validate();

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

    public override bool Equals(object obj)
    {
        if (obj is not SchemaElement otherSchema)
        {
            return false;
        }

        return this.Equals(otherSchema);
    }

    public abstract bool Equals(SchemaElement? other);

    public override abstract int GetHashCode();
}