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
        this.Modifier = MetadataAttributes.Extract(xElement, null).Modifier;
    }

    /// <summary>
    /// The XML element associated with this schema element. Contents may change as merge operations
    /// are applied.
    /// </summary>
    public XElement XmlElement { get; }

    /// <summary>
    /// Indicates if this element has the Optional attribute.
    /// </summary>
    public NodeModifier Modifier { get; private set; }

    /// <summary>
    /// Merges the given element with the schema, assuming it passes validation.
    /// </summary>
    public bool MergeWith(
        XElement element,
        bool allowAbstract,
        IErrorCollector? collector,
        IAttributeValidator attributeValidator)
    {
        collector = new DeduplicatingErrorCollector(collector);

        if (this.Modifier is NodeModifier.Final)
        {
            collector.Error(
                $"Cannot override element with the '{nameof(NodeModifier.Final)}' option",
                element);

            return false;
        }

        if (!this.MatchesSchema(element, MatchKind.Subset, attributeValidator, collector))
        {
            return false;
        }

        var attributes = attributeValidator.Validate(element, collector);
        this.SetModifier(attributes.Modifier);

        if (this.Modifier != NodeModifier.Abstract)
        {
            this.MergeWithProtected(element, allowAbstract, attributeValidator, collector);
        }

        // Finally, search for any abstract elements.
        if (!allowAbstract)
        {
            MarkAbstractDescendants(this, collector);
        }
        
        return !collector.HasErrors;
    }

    public void SetModifier(NodeModifier newModifier)
    {
        MixableInternal.Assert(
            this.Modifier is not NodeModifier.Final,
            "Can't change final modidifer");

        this.XmlElement.SetAttributeValue(Constants.Attributes.Flags, newModifier.ToString());
        this.Modifier = newModifier;

        if (newModifier == NodeModifier.Abstract)
        {
            this.OnSetAbstract();
            this.XmlElement.RemoveNodes();
        }
    }

    /// <summary>
    /// Recursively merges the given element into this one. Assumes that 
    /// <see cref="MatchesSchema(XElement, MatchKind, IAttributeValidator, IErrorCollector)"/>
    /// succeeded.
    /// </summary>
    protected abstract void MergeWithProtected(
        XElement element,
        bool allowAbstract,
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

    protected abstract void OnSetAbstract();

    public abstract T Accept<T>(ISchemaVisitor<T> visitor);

    public virtual IEnumerable<SchemaElement> GetEnumerator()
    {
        yield break;
    }

    private static void MarkAbstractDescendants(
        SchemaElement element,
        IErrorCollector errorCollector)
    {
        if (element.Modifier == NodeModifier.Abstract)
        {
            errorCollector.Error(
                "Abstract nodes are not permitted to remain after the final merge.",
                element.XmlElement);
        }

        foreach (SchemaElement item in element.GetEnumerator())
        {
            MarkAbstractDescendants(item, errorCollector);
        }
    }
}